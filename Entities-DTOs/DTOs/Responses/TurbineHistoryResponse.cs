using SEGEDE_Grupo1.EntitiesDTOs.Entities;

namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Responses;

// Historial completo de una turbina (§8.2).
public class TurbineHistoryResponse
{
    // Llave foránea (FK) que vincula este registro con su entidad padre relacional.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public List<TurbineStateHistory> StateChanges { get; set; } = new();
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public List<Maintenance> Maintenances { get; set; } = new();
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public List<Failure> Failures { get; set; } = new();
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal TotalGeneratedEnergy { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal TotalLostEnergy { get; set; }
}
