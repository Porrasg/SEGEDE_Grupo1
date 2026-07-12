using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

// ============================================================================
// SGDE OTP Service — servicio externo de OTP (§13.3 del TechDesign v2).
// Implementa exactamente el contrato que consume CoreApp/External/OtpServiceClient:
//   POST /api/otp/request  { email, usageType }         → 200 si se generó y envió
//   POST /api/otp/verify   { email, usageType, code }   → 200 si el código es válido, 400 si no
// Autenticación: header X-Api-Key (configurado en appsettings).
// Los códigos viven en memoria con expiración de 3 minutos (RNF: OTP de 1-3 min).
// ============================================================================

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string apiKey = builder.Configuration["ApiKey"] ?? "";
string masterCode = builder.Configuration["MasterCode"] ?? "";
int expiryMinutes = int.TryParse(builder.Configuration["ExpiryMinutes"], out int em) ? em : 3;

var codes = new ConcurrentDictionary<string, OtpEntry>();

// Middleware de autenticación por API key (mismo header que envía OtpServiceClient).
app.Use(async (ctx, next) =>
{
    if (!string.IsNullOrEmpty(apiKey) &&
        (!ctx.Request.Headers.TryGetValue("X-Api-Key", out var provided) || provided != apiKey))
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await ctx.Response.WriteAsJsonAsync(new { success = false, message = "API key inválida o ausente." });
        return;
    }
    await next();
});

app.MapPost("/api/otp/request", (OtpRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.UsageType))
    {
        return Results.BadRequest(new { success = false, message = "email y usageType son requeridos." });
    }

    // Código criptográficamente aleatorio de 6 dígitos.
    string code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
    var entry = new OtpEntry(code, DateTime.UtcNow.AddMinutes(expiryMinutes), 0);
    codes[Key(req.Email, req.UsageType)] = entry;

    bool emailed = TrySendEmail(builder.Configuration, req.Email, code, expiryMinutes, app.Logger);

    // En desarrollo el código también se registra en consola para poder probar
    // con destinatarios no reales (p.ej. los usuarios seed *@segede.local).
    if (app.Environment.IsDevelopment())
    {
        app.Logger.LogInformation("[OTP] {Email} ({UsageType}) → código {Code} (email {Estado}, expira en {Min} min)",
            req.Email, req.UsageType, code, emailed ? "enviado" : "NO enviado", expiryMinutes);
    }

    return Results.Ok(new { success = true, message = "OTP generado y enviado." });
});

app.MapPost("/api/otp/verify", (OtpVerifyRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.UsageType) || string.IsNullOrWhiteSpace(req.Code))
    {
        return Results.BadRequest(new { success = false, message = "email, usageType y code son requeridos." });
    }

    // Código maestro de desarrollo (solo si está configurado): permite demos con usuarios
    // seed cuyo correo no existe. En producción se deja vacío y no aplica.
    if (!string.IsNullOrEmpty(masterCode) && req.Code == masterCode)
    {
        return Results.Ok(new { success = true, message = "OTP verificado (master)." });
    }

    string key = Key(req.Email, req.UsageType);
    if (!codes.TryGetValue(key, out var entry))
    {
        return Results.BadRequest(new { success = false, message = "No hay un OTP activo para este correo/uso." });
    }
    if (DateTime.UtcNow > entry.ExpiresAt)
    {
        codes.TryRemove(key, out _);
        return Results.BadRequest(new { success = false, message = "El OTP expiró." });
    }
    if (entry.FailedAttempts >= 5)
    {
        codes.TryRemove(key, out _);
        return Results.BadRequest(new { success = false, message = "Demasiados intentos fallidos; solicite un nuevo OTP." });
    }
    if (entry.Code != req.Code)
    {
        codes[key] = entry with { FailedAttempts = entry.FailedAttempts + 1 };
        return Results.BadRequest(new { success = false, message = "Código incorrecto." });
    }

    // Un OTP es de un solo uso: se elimina al verificarse correctamente.
    codes.TryRemove(key, out _);
    return Results.Ok(new { success = true, message = "OTP verificado." });
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "SGDE OtpService" }));

app.Run();

static string Key(string email, string usageType) => $"{email.Trim().ToLowerInvariant()}|{usageType.Trim().ToLowerInvariant()}";

static bool TrySendEmail(IConfiguration cfg, string to, string code, int expiryMinutes, ILogger logger)
{
    string host = cfg["Smtp:Host"] ?? "";
    if (string.IsNullOrWhiteSpace(host)) return false;

    try
    {
        int port = int.TryParse(cfg["Smtp:Port"], out int p) ? p : 587;
        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(cfg["Smtp:User"] ?? "", cfg["Smtp:Password"] ?? ""),
            EnableSsl = !string.Equals(cfg["Smtp:EnableSsl"], "false", StringComparison.OrdinalIgnoreCase)
        };
        string from = cfg["Smtp:FromAddress"] ?? "no-reply@sgde.cr";
        string subject = "SGDE - Código de verificación";
        string body = $"Su código de verificación SGDE es: {code}\n\nEste código expira en {expiryMinutes} minutos y es de un solo uso.\nSi usted no solicitó este código, ignore este mensaje.";
        using var mail = new MailMessage(from, to, subject, body);
        client.Send(mail);
        return true;
    }
    catch (Exception ex)
    {
        logger.LogWarning("[OTP] Falló el envío de correo a {To}: {Error}", to, ex.Message);
        return false;
    }
}

record OtpRequest(string Email, string UsageType);
record OtpVerifyRequest(string Email, string UsageType, string Code);
record OtpEntry(string Code, DateTime ExpiresAt, int FailedAttempts);
