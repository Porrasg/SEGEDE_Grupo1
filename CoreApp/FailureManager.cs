using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

namespace SEGEDE_Grupo1.CoreApp;

// Manager de Fallas (§14.4). Instancia fábricas directamente con new sin IoC.
// Gestiona registro de fallas e impactos automáticos en el estado de la turbina si la severidad es crítica.
public class FailureManager
{
    private readonly FailureCrudFactory _failureCrudFactory = new();
    private readonly TurbineCrudFactory _turbineCrudFactory = new();
    private readonly AuditManager _auditManager = new();

    // RF-020: Registra una falla en una turbina. Si la severidad es Critical, la turbina pasa automáticamente a estado Damaged.
    public void Register(RegisterFailureRequest r, int callerUserId)
    {
        var turbine = _turbineCrudFactory.RetrieveById<Turbine>(r.TurbineId) ?? throw new NotFoundException("Turbine not found.");

        var now = TimeHelper.NowCR();
        var failure = new Failure
        {
            TurbineId = r.TurbineId,
            FailureDate = now,
            Description = r.Description,
            Severity = r.Severity,
            Created = now
        };

        _failureCrudFactory.Create(failure);

        if (string.Equals(r.Severity, FailureSeverities.Critical, StringComparison.OrdinalIgnoreCase))
        {
            if (turbine.Status != TurbineStates.Damaged && turbine.Status != TurbineStates.Decommissioned)
            {
                new TurbineManager().ChangeState(new ChangeTurbineStateRequest
                {
                    TurbineId = r.TurbineId,
                    NewState = TurbineStates.Damaged,
                    Reason = $"Critical failure reported: {r.Description}"
                }, callerUserId);
            }
        }

        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Failures, AuditActions.Create, "tblFailures", r.TurbineId, null, $"Reported {r.Severity} failure on turbine {turbine.UniqueCode}");
    }

    // RF-021: Retorna el historial de fallas de una turbina.
    public List<Failure> RetrieveByTurbine(int turbineId)
    {
        return _failureCrudFactory.RetrieveByTurbine(turbineId);
    }

    // Retorna todas las fallas reportadas en el parque eólico.
    public List<Failure> RetrieveAll()
    {
        return _failureCrudFactory.RetrieveAll<Failure>();
    }
}
