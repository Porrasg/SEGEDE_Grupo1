namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs;

// TODO: Estructura base para solicitudes paginadas segÃºn documento tÃ©cnico Â§3.4.
public class PagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
