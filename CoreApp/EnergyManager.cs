using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs.DTOs;
using SEGEDE_Grupo1.EntitiesDTOs;
using SEGEDE_Grupo1.EntitiesDTOs.Exceptions;

namespace SEGEDE_Grupo1.CoreApp;

// Manager de Energía (§14.5). Instancia fábricas directamente con new sin IoC.
// Ejecuta el ciclo de simulación de energía (cada 30s) calculando generación y pérdidas, y actualizando la batería local.
public class EnergyManager
{
    private readonly EnergyGenerationLogCrudFactory _genLogCrudFactory = new();
    private readonly EnergyLossLogCrudFactory _lossLogCrudFactory = new();
    private readonly LocalBatteryCrudFactory _localBatteryCrudFactory = new();

    // Recupera todas las baterías locales (helper usado por el controlador)
    public List<LocalBattery> RetrieveAllLocalBatteries()
    {
        return _localBatteryCrudFactory.RetrieveAll<LocalBattery>();
    }
    private readonly TurbineCrudFactory _turbineCrudFactory = new();

    // RF-025/026/027/028 (§17.3): Ejecuta ciclo de simulación de energía en ventana de 30s.
    // Calcula tiempo activo e inactivo según fecha del último cambio de estado, genera logs de generación y pérdida, y acumula en la batería.
    public void RunSimulationCycle()
    {
        var turbines = _turbineCrudFactory.RetrieveAll<Turbine>();
        var tCycle = TimeHelper.NowCR();
        var tStart = tCycle.AddSeconds(-30);
        const decimal secInWeek = 604800m; // 7 * 24 * 3600 segundos en una semana nominal

        foreach (var t in turbines)
        {
            decimal weeklyCap = t.WeeklyNominalCapacity;
            if (weeklyCap <= 0) continue;

            decimal ta = 0m;
            decimal ti = 0m;

            if (string.Equals(t.Status, TurbineStates.Active, StringComparison.OrdinalIgnoreCase))
            {
                ta = t.LastStateChange <= tStart ? 30m : (decimal)(tCycle - t.LastStateChange).TotalSeconds;
                if (ta > 30m) ta = 30m;
                if (ta < 0m) ta = 0m;
                ti = 30m - ta;

                if (ta > 0m)
                {
                    decimal eg = (weeklyCap / secInWeek) * ta;
                    _genLogCrudFactory.Create(new EnergyGenerationLog
                    {
                        TurbineId = t.Id,
                        ActiveTimeSeconds = Math.Round(ta, 2),
                        GeneratedEnergy = Math.Round(eg, 4),
                        EventDate = tCycle,
                        Created = tCycle
                    });

                    var battery = _localBatteryCrudFactory.RetrieveByTurbine(t.Id);
                    if (battery != null)
                    {
                        _localBatteryCrudFactory.UpdateEnergy(battery.Id, battery.StoredEnergy + eg, tCycle);
                    }
                }

                if (ti > 0m)
                {
                    decimal ep = (weeklyCap / secInWeek) * ti;
                    _lossLogCrudFactory.Create(new EnergyLossLog
                    {
                        TurbineId = t.Id,
                        InactiveTimeSeconds = Math.Round(ti, 2),
                        LostEnergy = Math.Round(ep, 4),
                        Cause = "Transition",
                        EventDate = tCycle,
                        Created = tCycle
                    });
                }
            }
            else
            {
                ti = t.LastStateChange <= tStart ? 30m : (decimal)(tCycle - t.LastStateChange).TotalSeconds;
                if (ti > 30m) ti = 30m;
                if (ti < 0m) ti = 0m;
                ta = 30m - ti;

                if (ti > 0m)
                {
                    decimal ep = (weeklyCap / secInWeek) * ti;
                    string cause = MapStatusToCause(t.Status);
                    _lossLogCrudFactory.Create(new EnergyLossLog
                    {
                        TurbineId = t.Id,
                        InactiveTimeSeconds = Math.Round(ti, 2),
                        LostEnergy = Math.Round(ep, 4),
                        Cause = cause,
                        EventDate = tCycle,
                        Created = tCycle
                    });
                }

                if (ta > 0m)
                {
                    decimal eg = (weeklyCap / secInWeek) * ta;
                    _genLogCrudFactory.Create(new EnergyGenerationLog
                    {
                        TurbineId = t.Id,
                        ActiveTimeSeconds = Math.Round(ta, 2),
                        GeneratedEnergy = Math.Round(eg, 4),
                        EventDate = tCycle,
                        Created = tCycle
                    });

                    var battery = _localBatteryCrudFactory.RetrieveByTurbine(t.Id);
                    if (battery != null)
                    {
                        _localBatteryCrudFactory.UpdateEnergy(battery.Id, battery.StoredEnergy + eg, tCycle);
                    }
                }
            }
        }
    }

    // RF-030: Retorna el estado actual de la batería local de una turbina.
    public LocalBattery RetrieveLocalBattery(int turbineId)
    {
        return _localBatteryCrudFactory.RetrieveByTurbine(turbineId) ?? throw new NotFoundException("Local battery not found for this turbine.");
    }

    // Simulator Panel (adenda v3 §131.2): permite forzar la carga de una batería local para reproducir
    // escenarios de prueba (saturación próxima, batería vacía) sin esperar ciclos de generación reales.
    public void SetLocalBatteryCharge(int turbineId, decimal storedEnergy)
    {
        if (storedEnergy < 0)
        {
            throw new BusinessException("Battery charge cannot be negative.", "INVALID_BATTERY_CHARGE");
        }

        var battery = _localBatteryCrudFactory.RetrieveByTurbine(turbineId) ?? throw new NotFoundException("Local battery not found for this turbine.");
        _localBatteryCrudFactory.UpdateEnergy(battery.Id, storedEnergy, TimeHelper.NowCR());
    }

    // RF-027: Retorna el historial paginado de logs de generación de una turbina.
    public PagedResponse<EnergyGenerationLog> RetrieveGenerationHistory(int turbineId, PagedRequest p)
    {
        var all = _genLogCrudFactory.RetrieveByTurbine(turbineId);
        var items = all.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToList();
        int totalPages = all.Count == 0 ? 0 : (int)Math.Ceiling(all.Count / (double)p.PageSize);

        return new PagedResponse<EnergyGenerationLog>
        {
            Items = items,
            Page = p.Page,
            PageSize = p.PageSize,
            TotalCount = all.Count,
            TotalPages = totalPages
        };
    }

    // RF-028: Retorna el historial paginado de logs de pérdidas de energía de una turbina.
    public PagedResponse<EnergyLossLog> RetrieveLossHistory(int turbineId, PagedRequest p)
    {
        var all = _lossLogCrudFactory.RetrieveByTurbine(turbineId);
        var items = all.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize).ToList();
        int totalPages = all.Count == 0 ? 0 : (int)Math.Ceiling(all.Count / (double)p.PageSize);

        return new PagedResponse<EnergyLossLog>
        {
            Items = items,
            Page = p.Page,
            PageSize = p.PageSize,
            TotalCount = all.Count,
            TotalPages = totalPages
        };
    }

    // Función operativa que ejecuta el procesamiento lógico y control del flujo de trabajo dentro de la capa actual.
    private static string MapStatusToCause(string status) => status switch
    {
        TurbineStates.UnderMaintenance => EnergyLossCauses.Maintenance,
        TurbineStates.Damaged => EnergyLossCauses.Failure,
        TurbineStates.SuspendedForNonCompliance => EnergyLossCauses.Suspension,
        TurbineStates.Decommissioned => EnergyLossCauses.Decommission,
        _ => "Other"
    };
}
