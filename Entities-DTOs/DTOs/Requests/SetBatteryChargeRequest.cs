namespace SEGEDE_Grupo1.EntitiesDTOs.DTOs.Requests;

// Forzar carga de batería local — Simulator Panel (adenda v3 §131.2).
public class SetBatteryChargeRequest
{
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public int TurbineId { get; set; }
    // Propiedad de datos mapeada a la columna de base de datos o parámetro de transferencia.
    public decimal StoredEnergy { get; set; }
}
