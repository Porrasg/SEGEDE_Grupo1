namespace SEGEDE_Grupo1.CoreApp.Helpers;

// Helper de tiempo con zona horaria America/Costa_Rica (§7.1).
// Regla: nunca usar DateTime.Now / DateTime.UtcNow en managers/factories.
// Siempre usar TimeHelper.NowCR().
public static class TimeHelper
{
    private static readonly TimeZoneInfo CR =
        TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    // Retorna la fecha y hora actual en la zona horaria de Costa Rica.
    public static DateTime NowCR() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CR);

    // Retorna true si la fecha dada es el último día del mes.
    public static bool IsLastDayOfMonth(DateTime date) =>
        date.Day == DateTime.DaysInMonth(date.Year, date.Month);
}
