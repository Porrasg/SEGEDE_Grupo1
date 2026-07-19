namespace SEGEDE_Grupo1.EntitiesDTOs;

// Mantenimiento de turbina → tblMaintenances (§9.6).
public class Maintenance : BaseDTO
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string MaintenanceType { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime EstimatedStartDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime EstimatedEndDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime? ActualStartDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime? ActualEndDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string? Result { get; set; }
    // Estado operativo actual o etapa en el ciclo de vida del registro.
    public string Status { get; set; } = string.Empty;
}
