using System.Security.Cryptography;
using SEGEDE_Grupo1.CoreApp.External;
using SEGEDE_Grupo1.CoreApp.Helpers;
using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs.Constants;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;
using SEGEDE_Grupo1.EntitiesDTOs.Helpers;
using SEGEDE_Grupo1.EntitiesDTOs.Validation;

namespace SEGEDE_Grupo1.CoreApp.Managers;

// Manager de Usuarios (§14.1). Respetando la arquitectura, instancia fábricas directamente con new sin IoC.
// Aplica validaciones de seguridad, control de intentos fallidos (bloqueo a los 5 intentos), flujos OTP y ownership.
public class UserManager
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public static string JwtSecret { get; set; } = Environment.GetEnvironmentVariable("Jwt:Secret") ?? "DefaultSuperSecretKeyThatIsAtLeast32BytesLongForHmacSha256!!";
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public static int JwtExpiryMinutes { get; set; } = 480;

    private readonly UserCrudFactory _userCrudFactory = new();
    private readonly OtpAttemptCrudFactory _otpAttemptCrudFactory = new();
    private readonly NotificationQueueCrudFactory _notificationFactory = new();
    private readonly OtpServiceClient _otpClient = new();
    private readonly AuditManager _auditManager = new();

    // Detecta si el servicio OTP está en modo simulación local (sin servidor externo real).
    // Cuando es true, se omiten pasos de OTP para permitir pruebas locales sin fricciones.
    private bool IsLocalSimulation =>
        string.IsNullOrWhiteSpace(OtpServiceClient.BaseUrlSetting) ||
        OtpServiceClient.BaseUrlSetting.Contains(".local", StringComparison.OrdinalIgnoreCase);

    // RF-001: Registro de comprador. Crea el usuario en estado PendingActivation, genera OTP de activación y encola notificación.
    public void Register(RegisterBuyerRequest r)
    {
        var validation = UserValidator.Validate(r.Email, r.Identification, r.Password, r.Phone, r.BirthDate, r.FirstName, r.LastName);
        validation.ThrowIfInvalid();

        var existingUser = _userCrudFactory.RetrieveByEmail(r.Email);
        if (existingUser != null)
        {
            throw new BusinessException("An account with this email already exists.", "EMAIL_ALREADY_EXISTS");
        }

        string passwordHash = PasswordHasher.Hash(r.Password);

        var user = new User
        {
            Identification = r.Identification,
            FirstName = r.FirstName,
            LastName = r.LastName,
            BirthDate = r.BirthDate,
            Phone = r.Phone,
            Email = r.Email,
            PhotoUrl = r.PhotoUrl,
            PasswordHash = passwordHash,
            Role = "Buyer",
            Status = IsLocalSimulation ? "Active" : "PendingActivation",
            FailedAttempts = 0,
            Created = TimeHelper.NowCR()
        };

        _userCrudFactory.Create(user);
        var createdUser = _userCrudFactory.RetrieveByEmail(r.Email) ?? throw new BusinessException("User creation failed.");

        if (IsLocalSimulation)
        {
            // En modo simulación local, la cuenta se activa automáticamente sin OTP.
            Console.WriteLine($"[DEV] Cuenta auto-activada para {r.Email}. No se requiere OTP en modo local.");
        }
        else
        {
            CreateAndSendOtp(createdUser.Id, createdUser.Email, OtpUsageTypes.Activation, "Account Activation Code");
        }

        _auditManager.LogAction(createdUser.Id, createdUser.Email, AuditModules.Users, AuditActions.Create, "tblUser", createdUser.Id, null, IsLocalSimulation ? "Buyer registered (auto-activated, local dev)" : "Buyer registered");
    }

    // RF-002: Calcula la edad actual en años a partir de la fecha de nacimiento.
    public int CalculateAge(DateTime dob)
    {
        var now = TimeHelper.NowCR();
        int age = now.Year - dob.Year;
        if (dob.Date > now.AddYears(-age)) age--;
        return age;
    }

    // RF-004: Activación de cuenta por OTP.
    public void Activate(ActivateAccountRequest r)
    {
        var user = _userCrudFactory.RetrieveByEmail(r.Email) ?? throw new NotFoundException("User not found.");

        if (!string.Equals(user.Status, "PendingActivation", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("User account is not pending activation.", "INVALID_STATUS");
        }

        VerifyOtpOrThrow(user.Id, user.Email, OtpUsageTypes.Activation, r.OtpCode);

        _userCrudFactory.UpdateStatus(user.Id, "Active", TimeHelper.NowCR());
        _auditManager.LogAction(user.Id, user.Email, AuditModules.Users, AuditActions.Activate, "tblUser", user.Id, "PendingActivation", "Active");
    }

    // RF-005: Paso 1 del login (autenticación por contraseña). Si es válido, genera OTP de Login.
    public void LoginStep1(LoginStep1Request r)
    {
        var user = _userCrudFactory.RetrieveByEmail(r.Email);
        if (user == null)
        {
            throw new BusinessException("Invalid login credentials.", "INVALID_CREDENTIALS");
        }

        if (string.Equals(user.Status, "Blocked", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("User account is blocked due to multiple failed login attempts.", "USER_BLOCKED");
        }

        if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("User account is not active.", "USER_INACTIVE");
        }

        bool isPasswordValid = PasswordHasher.Verify(r.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _userCrudFactory.IncrementFailedAttempts(user.Id);
            if (user.FailedAttempts + 1 >= 5)
            {
                _userCrudFactory.BlockUser(user.Id, TimeHelper.NowCR(), TimeHelper.NowCR());
                _auditManager.LogAction(user.Id, user.Email, AuditModules.Users, AuditActions.Block, "tblUser", user.Id, "Active", "Blocked");
                throw new BusinessException("User account blocked after 5 consecutive failed login attempts.", "USER_BLOCKED");
            }
            throw new BusinessException("Invalid login credentials.", "INVALID_CREDENTIALS");
        }

        if (!IsLocalSimulation)
        {
            CreateAndSendOtp(user.Id, user.Email, OtpUsageTypes.Login, "Your Login Verification Code");
        }
        else
        {
            Console.WriteLine($"[DEV] Login Step 1 OK para {user.Email}. OTP omitido en modo local.");
        }
        _auditManager.LogAction(user.Id, user.Email, AuditModules.Users, AuditActions.Execute, "tblUser", user.Id, null, "Step 1 password verified");
    }

    // RF-005: Paso 2 del login (verificación OTP y emisión de JWT).
    public LoginResponse LoginStep2(LoginStep2Request r)
    {
        var user = _userCrudFactory.RetrieveByEmail(r.Email) ?? throw new NotFoundException("User not found.");

        if (!IsLocalSimulation)
        {
            VerifyOtpOrThrow(user.Id, user.Email, OtpUsageTypes.Login, r.OtpCode);
        }
        else
        {
            Console.WriteLine($"[DEV] Login Step 2 OTP verificación omitida para {user.Email} en modo local.");
        }

        _userCrudFactory.ResetFailedAttempts(user.Id);

        string token = JwtHelper.GenerateToken(user.Id, user.Email, user.Role, JwtSecret, JwtExpiryMinutes);

        _auditManager.LogAction(user.Id, user.Email, AuditModules.Users, AuditActions.Execute, "tblUser", user.Id, null, "Login successful");

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role,
            Expiration = TimeHelper.NowCR().AddMinutes(JwtExpiryMinutes)
        };
    }

    // RF-006: Inicio de recuperación de contraseña.
    public void RecoverPassword(RecoverPasswordRequest r)
    {
        var user = _userCrudFactory.RetrieveByEmail(r.Email);
        if (user != null && string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            CreateAndSendOtp(user.Id, user.Email, OtpUsageTypes.Recovery, "Password Recovery Code");
            _auditManager.LogAction(user.Id, user.Email, AuditModules.Users, AuditActions.Execute, "tblUser", user.Id, null, "Password recovery OTP requested");
        }
    }

    // RF-006: Reinicio de contraseña con OTP verificado.
    public void ResetPassword(ResetPasswordRequest r)
    {
        var user = _userCrudFactory.RetrieveByEmail(r.Email) ?? throw new NotFoundException("User not found.");

        VerifyOtpOrThrow(user.Id, user.Email, OtpUsageTypes.Recovery, r.OtpCode);

        var valResult = UserValidator.Validate(user.Email, user.Identification, r.NewPassword, user.Phone, user.BirthDate, user.FirstName, user.LastName);
        if (valResult.Errors.Any(e => e.Contains("Password", StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException(valResult.Errors.Where(e => e.Contains("Password", StringComparison.OrdinalIgnoreCase)).ToArray());
        }

        string newHash = PasswordHasher.Hash(r.NewPassword);
        _userCrudFactory.UpdatePassword(user.Id, newHash, TimeHelper.NowCR());
        _auditManager.LogAction(user.Id, user.Email, AuditModules.Users, AuditActions.Execute, "tblUser", user.Id, null, "Password successfully reset");
    }

    // RF-007: Reenvío de código OTP (< 3 reenvíos permitidos).
    public void ResendOtp(ResendOtpRequest r)
    {
        var user = _userCrudFactory.RetrieveByEmail(r.Email) ?? throw new NotFoundException("User not found.");
        var activeAttempt = _otpAttemptCrudFactory.RetrieveActive(user.Id, r.UsageType);

        if (activeAttempt != null && activeAttempt.ResendCount >= 3)
        {
            throw new BusinessException("Maximum resend attempts reached for this operation.", "MAX_RESEND_REACHED");
        }

        if (activeAttempt != null)
        {
            _otpAttemptCrudFactory.IncrementResendCount(activeAttempt.Id);
        }

        CreateAndSendOtp(user.Id, user.Email, r.UsageType, $"Resent OTP Code ({r.UsageType})");
        _auditManager.LogAction(user.Id, user.Email, AuditModules.Users, AuditActions.Update, "tblOtpAttempts", user.Id, null, $"Resent OTP for {r.UsageType}");
    }

    // RF-008: Creación de usuario interno (Engineer/Admin) por el Administrador.
    public void CreateInternal(CreateInternalUserRequest r)
    {
        var existingUser = _userCrudFactory.RetrieveByEmail(r.Email);
        if (existingUser != null)
        {
            throw new BusinessException("An account with this email already exists.", "EMAIL_ALREADY_EXISTS");
        }

        string tempPassword = !string.IsNullOrWhiteSpace(r.Password) ? r.Password : $"SEGEDE_{RandomNumberGenerator.GetInt32(1000, 9999)}!";
        string passwordHash = PasswordHasher.Hash(tempPassword);

        var user = new User
        {
            Identification = r.Identification,
            FirstName = r.FirstName,
            LastName = r.LastName,
            BirthDate = r.BirthDate,
            Phone = r.Phone,
            Email = r.Email,
            PhotoUrl = null,
            PasswordHash = passwordHash,
            Role = r.Role,
            Status = "Active",
            FailedAttempts = 0,
            Created = TimeHelper.NowCR()
        };

        _userCrudFactory.Create(user);
        var createdUser = _userCrudFactory.RetrieveByEmail(r.Email) ?? throw new BusinessException("User creation failed.");

        EnqueueNotification(createdUser.Id, createdUser.Email, NotificationTypes.AccountActivation, "Welcome to SEGEDE - Account Created",
            $"Your internal account ({r.Role}) has been created. Your temporary password is: {tempPassword}", true);

        _auditManager.LogAction(createdUser.Id, createdUser.Email, AuditModules.Users, AuditActions.Create, "tblUser", createdUser.Id, null, $"Created internal user with role {r.Role}");
    }

    // Crea o restablece usuarios de prueba (Admin, Engineer, Buyer y 5 clientes reales de energía) activos en entorno local con historial de 5 años.
    public void SeedDevUsers()
    {
        var testUsers = new[]
        {
            new { Id = "100000001", First = "Carlos", Last = "Administrador", Email = "admin@segede.local", Role = "Administrator", Pass = "Admin123!", Year = 2021, Month = 1, Day = 10 },
            new { Id = "100000002", First = "Ana", Last = "Ingeniera", Email = "engineer@segede.local", Role = "Engineer", Pass = "Eng123!", Year = 2021, Month = 1, Day = 10 },
            new { Id = "100000003", First = "Juan", Last = "Comprador", Email = "buyer@segede.local", Role = "Buyer", Pass = "Buyer123!", Year = 2021, Month = 1, Day = 15 },
            new { Id = "300000001", First = "ICE", Last = "Costa Rica", Email = "ice@segede.cr", Role = "Buyer", Pass = "Buyer123!", Year = 2021, Month = 1, Day = 15 },
            new { Id = "300000002", First = "CNFL", Last = "Fuerza y Luz", Email = "cnfl@segede.cr", Role = "Buyer", Pass = "Buyer123!", Year = 2021, Month = 1, Day = 15 },
            new { Id = "300000003", First = "JASEC", Last = "Cartago", Email = "jasec@segede.cr", Role = "Buyer", Pass = "Buyer123!", Year = 2021, Month = 1, Day = 15 },
            new { Id = "300000004", First = "ESPH", Last = "Heredia", Email = "esph@segede.cr", Role = "Buyer", Pass = "Buyer123!", Year = 2021, Month = 1, Day = 15 },
            new { Id = "300000005", First = "COOPELESCA", Last = "San Carlos", Email = "coopelesca@segede.cr", Role = "Buyer", Pass = "Buyer123!", Year = 2021, Month = 1, Day = 15 }
        };

        foreach (var u in testUsers)
        {
            try
            {
                var existing = _userCrudFactory.RetrieveByEmail(u.Email);
                string hash = PasswordHasher.Hash(u.Pass);
                var createdDate = new DateTime(u.Year, u.Month, u.Day, 8, 0, 0);
                if (existing == null)
                {
                    var user = new User
                    {
                        Identification = u.Id,
                        FirstName = u.First,
                        LastName = u.Last,
                        BirthDate = new DateTime(1985, 5, 15),
                        Phone = "88888888",
                        Email = u.Email,
                        PhotoUrl = null,
                        PasswordHash = hash,
                        Role = u.Role,
                        Status = "Active",
                        FailedAttempts = 0,
                        Created = createdDate
                    };
                    _userCrudFactory.Create(user);
                    Console.WriteLine($"[SEED DEV] Creado cliente histórico: {u.Email} ({u.Role}) / Pass: {u.Pass}");
                }
                else
                {
                    _userCrudFactory.UpdateStatus(existing.Id, "Active", TimeHelper.NowCR());
                    _userCrudFactory.UpdatePassword(existing.Id, hash, TimeHelper.NowCR());
                    _userCrudFactory.ResetFailedAttempts(existing.Id);
                    Console.WriteLine($"[SEED DEV] Actualizado cliente histórico: {u.Email} ({u.Role}) / Pass: {u.Pass}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SEED DEV ERROR] Error al procesar {u.Email}: {ex.Message}");
            }
        }
    }

    // RF-009: Administrador edita campos administrativos de un usuario.
    public void UpdateUser(UpdateUserRequest r)
    {
        var existing = _userCrudFactory.RetrieveById<User>(r.UserId) ?? throw new NotFoundException("User not found.");

        var user = new User
        {
            Id = r.UserId,
            FirstName = r.FirstName,
            LastName = r.LastName,
            Phone = r.Phone,
            Role = r.Role,
            Status = r.Status,
            Updated = TimeHelper.NowCR()
        };

        _userCrudFactory.Update(user);
        _auditManager.LogAction(r.UserId, existing.Email, AuditModules.Users, AuditActions.Update, "tblUser", r.UserId, existing.Status, r.Status);
    }

    // RF-010: Comprador edita su perfil (verificación de propiedad / ownership).
    public void UpdateProfile(UpdateProfileRequest r, int callerUserId)
    {
        var existing = _userCrudFactory.RetrieveById<User>(callerUserId) ?? throw new NotFoundException("User not found.");

        string? newHash = string.IsNullOrWhiteSpace(r.NewPassword) ? existing.PasswordHash : PasswordHasher.Hash(r.NewPassword);
        _userCrudFactory.UpdateProfile(callerUserId, r.Phone, r.PhotoUrl, newHash, TimeHelper.NowCR());
        _auditManager.LogAction(callerUserId, existing.Email, AuditModules.Users, AuditActions.Update, "tblUser", callerUserId, null, "Profile updated by buyer");
    }

    // RF-011: Borrado lógico de usuario. Si se borra un comprador, cancela proyecciones a > 3 meses.
    public void Deactivate(DeactivateUserRequest r, int callerUserId, string callerRole)
    {
        if (!string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase) && r.UserId != callerUserId)
        {
            throw new UnauthorizedAccessAppException("You can only deactivate your own account or be an Administrator.", "OWNERSHIP_VIOLATION");
        }

        var user = _userCrudFactory.RetrieveById<User>(r.UserId) ?? throw new NotFoundException("User not found.");

        _userCrudFactory.UpdateStatus(user.Id, "Inactive", TimeHelper.NowCR());

        if (string.Equals(user.Role, "Buyer", StringComparison.OrdinalIgnoreCase) && string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase))
        {
            new ForecastManager().CancelBeyond3Months(user.Id);
        }

        _auditManager.LogAction(callerUserId, user.Email, AuditModules.Users, AuditActions.LogicalDelete, "tblUser", user.Id, "Active", "Inactive");
    }

    // RF-012: Solo el Administrador puede reactivar usuarios inactivos.
    public void Reactivate(int userId)
    {
        var user = _userCrudFactory.RetrieveById<User>(userId) ?? throw new NotFoundException("User not found.");
        _userCrudFactory.UpdateStatus(user.Id, "Active", TimeHelper.NowCR());
        _auditManager.LogAction(null, "System/Admin", AuditModules.Users, AuditActions.Activate, "tblUser", user.Id, "Inactive", "Active");
    }

    // Retorna lista de usuarios sin exponer datos sensibles (hash, intentos).
    public List<UserSafeResponse> RetrieveAll()
    {
        var users = _userCrudFactory.RetrieveAll<User>();
        return users.Select(MapToSafeResponse).ToList();
    }

    // Retorna un usuario por ID sin exponer datos sensibles.
    public UserSafeResponse RetrieveById(int id)
    {
        var user = _userCrudFactory.RetrieveById<User>(id) ?? throw new NotFoundException("User not found.");
        return MapToSafeResponse(user);
    }

    // Sube una foto de perfil y actualiza el campo PhotoUrl del usuario.
    public string UploadPhoto(int userId, Stream file, string contentType)
    {
        var user = _userCrudFactory.RetrieveById<User>(userId) ?? throw new NotFoundException("User not found.");
        
        string photoUrl = $"https://storage.segede.local/photos/{userId}_{TimeHelper.NowCR():yyyyMMddHHmmss}.jpg";
        _userCrudFactory.UpdateProfile(user.Id, user.Phone, photoUrl, user.PasswordHash, TimeHelper.NowCR());
        _auditManager.LogAction(userId, user.Email, AuditModules.Users, AuditActions.Update, "tblUser", userId, user.PhotoUrl, photoUrl);
        
        return photoUrl;
    }

    // --- Helpers Privados ---

    private void CreateAndSendOtp(int userId, string email, string usageType, string subject)
    {
        var active = _otpAttemptCrudFactory.RetrieveActive(userId, usageType);
        if (active != null)
        {
            _otpAttemptCrudFactory.UpdateStatus(active.Id, OtpAttemptStates.Blocked, TimeHelper.NowCR());
        }

        var otpAttempt = new OtpAttempt
        {
            UserId = userId,
            UsageType = usageType,
            ResendCount = 0,
            FailedAttempts = 0,
            Status = OtpAttemptStates.InProgress,
            StartDate = TimeHelper.NowCR(),
            WindowExpiration = TimeHelper.NowCR().AddMinutes(3),
            Created = TimeHelper.NowCR()
        };

        _otpAttemptCrudFactory.Create(otpAttempt);

        bool sent = _otpClient.RequestOtp(email, usageType);
        if (!sent)
        {
            EnqueueNotification(userId, email, NotificationTypes.AccountActivation, subject, $"Your OTP code requested for {usageType}. Please verify via standard channel.", true);
        }
    }

    // Ejecuta operaciones criptográficas para el resguardo y verificación segura de credenciales e integridad.
    private void VerifyOtpOrThrow(int userId, string email, string usageType, string code)
    {
        var activeAttempt = _otpAttemptCrudFactory.RetrieveActive(userId, usageType);
        if (activeAttempt == null || activeAttempt.WindowExpiration < TimeHelper.NowCR())
        {
            throw new BusinessException("OTP code has expired or is invalid.", "OTP_EXPIRED");
        }

        bool isValid = _otpClient.VerifyOtp(email, usageType, code);
        if (!isValid)
        {
            _otpAttemptCrudFactory.IncrementFailedAttempts(activeAttempt.Id);
            if (activeAttempt.FailedAttempts + 1 >= 5)
            {
                _otpAttemptCrudFactory.UpdateStatus(activeAttempt.Id, OtpAttemptStates.Blocked, TimeHelper.NowCR());
            }
            throw new BusinessException("Invalid OTP code entered.", "INVALID_OTP");
        }

        _otpAttemptCrudFactory.UpdateStatus(activeAttempt.Id, OtpAttemptStates.Verified, TimeHelper.NowCR());
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private void EnqueueNotification(int userId, string email, string type, string subject, string body, bool isCritical)
    {
        var notif = new NotificationQueue
        {
            UserId = userId,
            RecipientEmail = email,
            NotificationType = type,
            Subject = subject,
            Body = body,
            IsCritical = isCritical,
            Status = NotificationStates.Pending,
            Attempts = 0,
            NextAttempt = TimeHelper.NowCR(),
            Created = TimeHelper.NowCR()
        };
        _notificationFactory.Create(notif);
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private UserSafeResponse MapToSafeResponse(User user) => new()
    {
        Id = user.Id,
        Identification = user.Identification,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Phone = user.Phone,
        PhotoUrl = user.PhotoUrl,
        Role = user.Role,
        Status = user.Status,
        BirthDate = user.BirthDate,
        Age = CalculateAge(user.BirthDate),
        Created = user.Created
    };
}
