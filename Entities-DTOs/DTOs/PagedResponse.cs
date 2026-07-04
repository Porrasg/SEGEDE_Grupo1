namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

// Respuesta paginada estándar (§3.4).
public class PagedResponse<T>
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public List<T> Items { get; set; } = new();
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int Page { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int PageSize { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int TotalCount { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int TotalPages { get; set; }
}
