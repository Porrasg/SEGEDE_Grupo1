using SEGEDE_Grupo1.CoreApp;
using SEGEDE_Grupo1.EntitiesDTOs;

namespace SEGEDE_Grupo1.WebAPI.BackgroundServices;

// Servicio de fondo que ejecuta el vaciado automático de baterías hacia el Banco Central
// según la hora y configuración guardadas en tblFlushConfig (§17.2).
// Se despierta cada minuto, verifica si es hora de ejecutar y si ya se ejecutó hoy.
public class AutoFlushJob : JobBase
{
    private readonly ILogger<AutoFlushJob> _logger;
    private readonly FlushManager _flushManager;

    public AutoFlushJob(ILogger<AutoFlushJob> logger)
    {
        _logger = logger;
        _flushManager = new FlushManager();
    }

    // Función operativa asíncrona que verifica cada minuto si corresponde ejecutar el flush automático.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio en segundo plano AutoFlushJob iniciado.");
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunGuarded(async () =>
            {
                try
                {
                    var config = _flushManager.GetFlushConfig();

                    // Si el modo automático está desactivado, no hacer nada.
                    if (!config.IsAutomatic)
                    {
                        await Task.CompletedTask;
                        return;
                    }

                    var now = TimeHelper.NowCR();
                    var scheduledTime = config.ExecutionTime;

                    // Verificar si ya pasó o llegó la hora configurada para hoy.
                    bool isTimeToRun = now.TimeOfDay >= scheduledTime;
                    if (!isTimeToRun)
                    {
                        await Task.CompletedTask;
                        return;
                    }

                    // Verificar en la base de datos si ya se ejecutó un flush automático hoy.
                    // Esto protege contra ejecuciones dobles si el servidor se reinicia después del flush.
                    var history = _flushManager.RetrieveFlushHistory(new EntitiesDTOs.DTOs.PagedRequest { Page = 1, PageSize = 50 });
                    bool alreadyRanToday = history.Items.Any(f =>
                        string.Equals(f.ExecutionType, FlushTypes.Automatic, StringComparison.OrdinalIgnoreCase) &&
                        f.StartDate.Date == now.Date);

                    if (alreadyRanToday)
                    {
                        await Task.CompletedTask;
                        return;
                    }

                    _logger.LogInformation("AutoFlushJob: ejecutando flush automático programado para las {Hora}.", scheduledTime);
                    _flushManager.ExecuteAutoFlush();
                    _logger.LogInformation("AutoFlushJob: flush automático completado exitosamente.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AutoFlushJob: error durante la ejecución del flush automático.");
                }
                await Task.CompletedTask;
            });

            // Revisar cada minuto si es hora de ejecutar.
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        _logger.LogInformation("Servicio en segundo plano AutoFlushJob detenido.");
    }
}
