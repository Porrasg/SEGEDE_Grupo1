namespace SEGEDE_Grupo1.WebAPI.BackgroundServices;

// TODO: Trabajo automÃ¡tico en segundo plano para simular generaciÃ³n de energÃ­a cada 30 segundos segÃºn Â§17.3.
public class EnergySimulationJob : JobBase
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
