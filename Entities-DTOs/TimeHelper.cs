namespace SEGEDE_Grupo1.EntitiesDTOs;

// Helper de tiempo con zona horaria America/Costa_Rica (§7.1).
// Regla: nunca usar DateTime.Now / DateTime.UtcNow en managers/factories.
// Siempre usar TimeHelper.NowCR().
public static class TimeHelper
{
    private static readonly TimeZoneInfo CR =
        TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public static DateTime NowCR() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CR);

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public static bool IsLastDayOfMonth(DateTime date) =>
        date.Day == DateTime.DaysInMonth(date.Year, date.Month);
}
