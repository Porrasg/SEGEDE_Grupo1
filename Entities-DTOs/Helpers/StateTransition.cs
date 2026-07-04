using SEGEDE_Grupo1.EntitiesDTOs.Constants;

namespace SEGEDE_Grupo1.EntitiesDTOs.Helpers;

/// <summary>
/// Matriz de transición de estados de turbinas (§7.2).
/// Valida qué cambios de estado son permitidos.
/// </summary>
public static class StateTransition
{
    private static readonly Dictionary<string, HashSet<string>> Allowed = new()
    {
        [TurbineStates.Active] = new()
        {
            TurbineStates.UnderMaintenance,
            TurbineStates.Damaged,
            TurbineStates.SuspendedForNonCompliance,
            TurbineStates.Decommissioned
        },
        [TurbineStates.UnderMaintenance] = new()
        {
            TurbineStates.Active,
            TurbineStates.Damaged,
            TurbineStates.Decommissioned
        },
        [TurbineStates.Damaged] = new()
        {
            TurbineStates.UnderMaintenance,   // NO directo a Active (RF-016)
            TurbineStates.Decommissioned
        },
        [TurbineStates.SuspendedForNonCompliance] = new()
        {
            TurbineStates.UnderMaintenance,
            TurbineStates.Decommissioned
        },
        [TurbineStates.Decommissioned] = new()   // terminal (RN-007)
    };

    public static bool IsValid(string from, string to) =>
        Allowed.TryGetValue(from, out var set) && set.Contains(to);
}
