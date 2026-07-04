namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Registro de mantenimiento (RF-017, §8.1).
public class RegisterMaintenanceRequest
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string MaintenanceType { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime EstimatedStartDate { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public DateTime EstimatedEndDate { get; set; }
}
