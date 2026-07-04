namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// Historial de estados de turbina → tblTurbineStateHistory (§9.4).
public class TurbineStateHistory : BaseDTO
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string PreviousState { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string NewState { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime ChangeDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Reason { get; set; } = string.Empty;
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int UserId { get; set; }
}
