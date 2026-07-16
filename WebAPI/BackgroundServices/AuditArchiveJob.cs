using SEGEDE_Grupo1.CoreApp.Managers;

namespace SEGEDE_Grupo1.WebAPI.BackgroundServices;

// Servicio de fondo que realiza el archivado en frío de bitácoras de auditoría antiguas respetando el esquema WORM (§14.12).
public class AuditArchiveJob : JobBase
{
    private readonly ILogger<AuditArchiveJob> _logger;
    private readonly AuditManager _auditManager = new();

    // Constructor del servicio de fondo para archivo de auditoría inmutable en frío.
    public AuditArchiveJob(ILogger<AuditArchiveJob> logger)
    {
        _logger = logger;
    }

    // Función operativa asíncrona que verifica y transfiere registros de auditoría antiguos cada 24 horas.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio en segundo plano AuditArchiveJob iniciado.");
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunGuarded(async () =>
            {
                try
                {
                    _auditManager.ArchiveColdRecords();
                    _logger.LogInformation("Se ejecutó el archivado en frío de registros de auditoría.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el archivado en frío de auditoría.");
                }
                await Task.CompletedTask;
            });
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
        _logger.LogInformation("Servicio en segundo plano AuditArchiveJob detenido.");
    }
}
