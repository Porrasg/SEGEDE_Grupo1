using SEGEDE_Grupo1.EntitiesDTOs.Constants;

namespace SEGEDE_Grupo1.EntitiesDTOs.Helpers;

// Matriz de transición de estados de turbinas (§7.2).
// Valida qué cambios de estado son permitidos.
public static class StateTransition
{
    // Ejecuta operaciones criptográficas para el resguardo y verificación segura de credenciales e integridad.
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

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    public static bool IsValid(string from, string to) =>
        Allowed.TryGetValue(from, out var set) && set.Contains(to);
}
