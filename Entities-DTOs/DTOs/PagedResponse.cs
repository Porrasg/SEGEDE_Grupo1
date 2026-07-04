namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

/// <summary>
/// Respuesta paginada estándar (§3.4).
/// </summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
