namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

// TODO: Estructura base para respuestas paginadas segÃºn documento tÃ©cnico Â§3.4.
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
