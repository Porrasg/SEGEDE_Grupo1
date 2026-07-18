using SEGEDE_Grupo1.CoreApp.Managers;

namespace SEGEDE_Grupo1.WebAPI.BackgroundServices;

// Servicio de fondo que ejecuta el ciclo de simulación de energía periódicamente según §14.5.
public class EnergySimulationJob : JobBase
{
    private readonly ILogger<EnergySimulationJob> _logger;
    private readonly EnergyManager _energyManager = new();

    // Constructor del servicio en segundo plano de simulación de energía.
    public EnergySimulationJob(ILogger<EnergySimulationJob> logger)
    {
        _logger = logger;
    }

    // Función operativa asíncrona que ejecuta el ciclo de simulación de energía cada 30 segundos mientras el servidor esté activo.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Servicio en segundo plano EnergySimulationJob iniciado.");
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunGuarded(async () =>
            {
                try
                {
                    _energyManager.RunSimulationCycle();
                    _logger.LogInformation("Ciclo de simulación de energía ejecutado satisfactoriamente.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error durante la ejecución del ciclo de simulación de energía.");
                }
                await Task.CompletedTask;
            });
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
        _logger.LogInformation("Servicio en segundo plano EnergySimulationJob detenido.");
    }
}
