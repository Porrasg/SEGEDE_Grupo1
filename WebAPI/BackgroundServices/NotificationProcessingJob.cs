using SEGEDE_Grupo1.CoreApp.Managers;

namespace SEGEDE_Grupo1.WebAPI.BackgroundServices;

// Servicio de fondo que procesa la cola asíncrona de notificaciones por correo electrónico periódicamente según §14.11.
public class NotificationProcessingJob : BackgroundService
{
    private readonly ILogger<NotificationProcessingJob> _logger;
    private readonly NotificationManager _notificationManager = new();

    // Constructor del servicio en segundo plano para envío de notificaciones en cola.
    public NotificationProcessingJob(ILogger<NotificationProcessingJob> logger)
    {
        _logger = logger;
    }

    // Función operativa asíncrona que escanea la cola de notificaciones y envía correos con backoff exponencial cada 60 segundos.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio en segundo plano NotificationProcessingJob iniciado.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _notificationManager.ProcessQueue();
                _logger.LogDebug("Procesamiento de cola de notificaciones completado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el procesamiento de la cola de notificaciones.");
            }
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
        _logger.LogInformation("Servicio en segundo plano NotificationProcessingJob detenido.");
    }
}
