namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

// Solicitud de paginación estándar (§3.4). Page 1-indexed, PageSize máx 200.
public class PagedRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int Page { get; set; } = 1;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int PageSize { get; set; } = 50;
}
