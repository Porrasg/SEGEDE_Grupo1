using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

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

    public OtpServiceClient()
    {
        _baseUrl = BaseUrlSetting;
        _apiKey = ApiKeySetting;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

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
        catch (HttpRequestException)
        {
            throw new BusinessException("OTP service unavailable.", "OTP_SERVICE_UNAVAILABLE");
        }
        catch (TaskCanceledException)
        {
            throw new BusinessException("OTP service unavailable.", "OTP_SERVICE_UNAVAILABLE");
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            throw new BusinessException("OTP service unavailable.", "OTP_SERVICE_UNAVAILABLE");
        }
    }

    // Verifica si el código OTP proporcionado es válido para el correo y tipo de uso.
    // Parámetro email: Correo del usuario.
    // Parámetro usageType: Tipo de uso.
    // Parámetro code: Código OTP de 6 dígitos ingresado por el usuario.
    // Retorna: True si el código es válido; de lo contrario, False.
    public bool VerifyOtp(string email, string usageType, string code)
    {
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
        catch (HttpRequestException)
        {
            throw new BusinessException("OTP service unavailable.", "OTP_SERVICE_UNAVAILABLE");
        }
        catch (TaskCanceledException)
        {
            throw new BusinessException("OTP service unavailable.", "OTP_SERVICE_UNAVAILABLE");
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            throw new BusinessException("OTP service unavailable.", "OTP_SERVICE_UNAVAILABLE");
        }
    }
}
