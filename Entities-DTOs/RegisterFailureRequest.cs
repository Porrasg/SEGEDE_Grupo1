namespace SEGEDE_Grupo1.EntitiesDTOs;

// Registro de falla (RF-020, §8.1).
public class RegisterFailureRequest
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Description { get; set; } = string.Empty;
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public string Severity { get; set; } = string.Empty;
}
