namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

/// <summary>
/// Solicitud de paginación estándar (§3.4). Page 1-indexed, PageSize máx 200.
/// </summary>
public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
