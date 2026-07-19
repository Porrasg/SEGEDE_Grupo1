using System.Threading;

namespace SEGEDE_Grupo1.WebAPI.BackgroundServices;

// Clase base abstracta para trabajos en segundo plano (Jobs automáticos) según documento técnico §17.
// Provee un helper RunGuarded para evitar ejecuciones superpuestas dentro del mismo proceso.
public abstract class JobBase : BackgroundService
{
    private int _isRunning = 0;

    // Ejecuta el delegado work solo si no hay otra ejecución en curso en este proceso.
    // Si ya está en ejecución, retorna inmediatamente.
    protected async Task RunGuarded(Func<Task> work)
    {
        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0) return;
        try
        {
            await work();
        }
        finally
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }
    }
}
