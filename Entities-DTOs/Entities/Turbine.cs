namespace SEGEDE_Grupo1.EntitiesDTOs.Entities;

// TODO: Entidad Turbine mapeada a tblTurbines segÃºn documento tÃ©cnico Â§9.3.
public class Turbine : BaseDTO
{
    public string UniqueCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal WeeklyNominalCapacity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastMaintenance { get; set; }
    public DateTime LastStateChange { get; set; }
}
