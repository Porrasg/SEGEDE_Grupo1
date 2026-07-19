using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;
using SEGEDE_Grupo1.EntitiesDTOs.Validation;

namespace SEGEDE_Grupo1.CoreApp;

// Manager de Pronósticos de Demanda (§14.8).
// Instanciación directa de fábricas sin IoC. Respeto al ownership de recursos del comprador.
public class ForecastManager
{
    private readonly ForecastCrudFactory _forecastCrudFactory = new();
    private readonly AuditManager _auditManager = new();

    // RF-044: Registro de pronóstico. Valida fecha futura, horizonte y que no exista un pronóstico duplicado activo.
    public void Register(RegisterForecastRequest r, int callerUserId)
    {
        ForecastValidator.Validate(r.AmountMWh, r.Month, r.Year).ThrowIfInvalid();

        var now = TimeHelper.NowCR();
        var existing = _forecastCrudFactory.RetrieveActiveByBuyerMonth(callerUserId, r.Month, r.Year);
        if (existing != null)
        {
            throw new BusinessException("An active forecast already exists for this month and year.", "DUPLICATE_FORECAST");
        }

        var forecast = new Forecast
        {
            BuyerId = callerUserId,
            Month = r.Month,
            Year = r.Year,
            AmountMWh = r.AmountMWh,
            Status = "Pending",
            Created = now
        };

        _forecastCrudFactory.Create(forecast);
        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Forecasts, AuditActions.Create, "tblForecast", 0, null, $"Registered forecast {r.AmountMWh} MWh for {r.Month}/{r.Year}");
    }

    // RF-046: Modificación de pronóstico. Verifica ownership y estado no bloqueado.
    public void Modify(ModifyForecastRequest r, int callerUserId, string callerRole)
    {
        var forecast = _forecastCrudFactory.RetrieveById<Forecast>(r.ForecastId) ?? throw new NotFoundException("Forecast not found.");

        if (!string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase) && forecast.BuyerId != callerUserId)
        {
            throw new UnauthorizedAccessAppException("You can only modify your own forecasts.", "OWNERSHIP_VIOLATION");
        }

        if (string.Equals(forecast.Status, "Blocked", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(forecast.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(forecast.Status, ForecastStates.Distributed, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Forecast cannot be modified in its current state.", "FORECAST_LOCKED");
        }

        if (r.NewAmountMWh <= 0)
        {
            throw new BusinessException("Forecast amount must be greater than zero.", "INVALID_AMOUNT");
        }

        string oldVal = $"{forecast.AmountMWh} MWh ({forecast.Status})";
        forecast.AmountMWh = r.NewAmountMWh;
        forecast.Status = "Modified";
        forecast.Updated = TimeHelper.NowCR();

        _forecastCrudFactory.Update(forecast);
        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Forecasts, AuditActions.Update, "tblForecast", forecast.Id, oldVal, $"{forecast.AmountMWh} MWh (Modified)");
    }

    // RF-047: Cancelación de pronóstico. Verifica ownership y estado no bloqueado.
    public void Cancel(int forecastId, int callerUserId, string callerRole)
    {
        var forecast = _forecastCrudFactory.RetrieveById<Forecast>(forecastId) ?? throw new NotFoundException("Forecast not found.");

        if (!string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase) && forecast.BuyerId != callerUserId)
        {
            throw new UnauthorizedAccessAppException("You can only cancel your own forecasts.", "OWNERSHIP_VIOLATION");
        }

        if (string.Equals(forecast.Status, "Blocked", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(forecast.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(forecast.Status, ForecastStates.Distributed, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Forecast cannot be cancelled in its current state.", "FORECAST_LOCKED");
        }

        _forecastCrudFactory.UpdateStatus(forecastId, "Cancelled", TimeHelper.NowCR());
        _auditManager.LogAction(callerUserId, $"User {callerUserId}", AuditModules.Forecasts, AuditActions.LogicalDelete, "tblForecast", forecastId, forecast.Status, "Cancelled");
    }

    // RF-049: Obtener pronósticos por comprador con validación de ownership.
    public List<Forecast> RetrieveByBuyer(int buyerId, int callerUserId, string callerRole)
    {
        if (!string.Equals(callerRole, "Administrator", StringComparison.OrdinalIgnoreCase) && buyerId != callerUserId)
        {
            throw new UnauthorizedAccessAppException("You can only view your own forecasts.", "OWNERSHIP_VIOLATION");
        }

        return _forecastCrudFactory.RetrieveByBuyer(buyerId);
    }

    // RF-050: Obtener pronósticos por mes y año (para distribución comercial).
    public List<Forecast> RetrieveByMonth(int month, int year)
    {
        return _forecastCrudFactory.RetrieveByMonth(month, year);
    }

    // RF-048: Cancela los pronósticos más allá de 3 meses en el futuro tras inactivar a un usuario.
    public void CancelBeyond3Months(int buyerId)
    {
        var thresholdDate = TimeHelper.NowCR().AddMonths(3);
        _forecastCrudFactory.CancelBeyond3Months(buyerId, thresholdDate.Month, thresholdDate.Year, TimeHelper.NowCR());
        _auditManager.LogAction(null, "System/Admin", AuditModules.Forecasts, AuditActions.LogicalDelete, "tblForecast", buyerId, null, "Cancelled forecasts beyond 3 months due to user deactivation");
    }
}
