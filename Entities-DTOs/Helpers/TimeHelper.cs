namespace SEGEDE_Grupo1.EntitiesDTOs.Helpers;

// TODO: Helper de tiempo con zona horaria America/Costa_Rica segÃºn documento tÃ©cnico Â§7.1.
public static class TimeHelper
{
    public static DateTime NowCR()
    {
        return DateTime.UtcNow;
    }
}
