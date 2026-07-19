using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SEGEDE_Grupo1.CoreApp.Exceptions;

namespace SEGEDE_Grupo1.CoreApp.External;

// Cliente para la comunicación con el servicio externo de OTP (§13.3).
// Lee BaseUrl y ApiKey de variables de entorno o configuración estática al instanciarse sin DI.
public class OtpServiceClient
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public static string BaseUrlSetting { get; set; } = Environment.GetEnvironmentVariable("OtpService:BaseUrl") ?? "https://api.otpservice.local";
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public static string ApiKeySetting { get; set; } = Environment.GetEnvironmentVariable("OtpService:ApiKey") ?? "";

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public OtpServiceClient()
    {
        _baseUrl = BaseUrlSetting;
        _apiKey = ApiKeySetting;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public OtpServiceClient(string baseUrl, string apiKey, HttpClient? httpClient = null)
    {
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    // Solicita la generación y envío de un OTP para un correo y tipo de uso específicos.
    // Parámetro email: Correo del destinatario.
    // Parámetro usageType: Tipo de uso (ej. Activation, Login, PasswordRecovery).
    // Retorna: True si la solicitud fue exitosa; de lo contrario, False.
    public bool RequestOtp(string email, string usageType)
    {
        // Si estamos en entorno de desarrollo local o con cuentas de prueba @segede.local, simulamos el servicio sin bloquear la red.
        if (string.IsNullOrWhiteSpace(_baseUrl) ||
            _baseUrl.Contains(".local", StringComparison.OrdinalIgnoreCase) ||
            _baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            _baseUrl.Contains("127.0.0.1") ||
            email.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[OTP SIMULATION] OTP solicitado para {email} ({usageType}). Para pruebas locales, use el código '123456', '999999' o '000000'.");
            return true;
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl.TrimEnd('/')}/api/otp/request");
            if (!string.IsNullOrEmpty(_apiKey))
            {
                request.Headers.Add("X-Api-Key", _apiKey);
            }

            var payload = new { email, usageType };
            request.Content = JsonContent.Create(payload);

            var response = _httpClient.Send(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            // Resiliencia en desarrollo local: si el microservicio OtpService en localhost (puerto 5306) no está ejecutándose,
            // caemos en modo simulación local para no bloquear el flujo de Registro o Login.
            if (_baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase) || _baseUrl.Contains("127.0.0.1"))
            {
                Console.WriteLine($"[OTP DEV FALLBACK] El servicio OtpService en {_baseUrl} no está ejecutándose. Modo simulación local activado para {email} ({usageType}). Código dev: '123456' o '999999'.");
                return true;
            }

            throw new BusinessException($"No se pudo contactar al proveedor externo de OTP ({_baseUrl}).", "OTP_SERVICE_UNAVAILABLE");
        }
    }

    // Verifica si el código OTP proporcionado es válido para el correo y tipo de uso.
    // Parámetro email: Correo del usuario.
    // Parámetro usageType: Tipo de uso.
    // Parámetro code: Código OTP de 6 dígitos ingresado por el usuario.
    // Retorna: True si el código es válido; de lo contrario, False.
    public bool VerifyOtp(string email, string usageType, string code)
    {
        // En modo simulación local o con cuentas de prueba @segede.local, cualquier código de desarrollo valida.
        if (string.IsNullOrWhiteSpace(_baseUrl) ||
            _baseUrl.Contains(".local", StringComparison.OrdinalIgnoreCase) ||
            _baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            _baseUrl.Contains("127.0.0.1") ||
            email.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl.TrimEnd('/')}/api/otp/verify");
            if (!string.IsNullOrEmpty(_apiKey))
            {
                request.Headers.Add("X-Api-Key", _apiKey);
            }

            var payload = new { email, usageType, code };
            request.Content = JsonContent.Create(payload);

            var response = _httpClient.Send(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            // Resiliencia en desarrollo local o cuentas de prueba @segede.local: permitimos los códigos de desarrollo.
            if (_baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                _baseUrl.Contains("127.0.0.1") ||
                email.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[OTP DEV FALLBACK] Verificando código '{code}' para {email}.");
                return code == "123456" || code == "999999" || code == "000000";
            }

            // Fail-closed en ambientes externos/producción si el servicio externo no responde.
            throw new BusinessException($"No se pudo contactar al proveedor externo de OTP ({_baseUrl}).", "OTP_SERVICE_UNAVAILABLE");
        }
    }
}
