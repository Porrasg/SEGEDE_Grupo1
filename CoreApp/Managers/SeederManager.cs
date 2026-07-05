using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs.Constants;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;
using SEGEDE_Grupo1.EntitiesDTOs.Helpers;
using SEGEDE_Grupo1.CoreApp.Helpers;

namespace SEGEDE_Grupo1.CoreApp.Managers;

// Manager encargado de poblar automáticamente la base de datos con ítems ficticios realistas para pruebas funcionales (§14.13, RN-030).
public class SeederManager
{
    private readonly TurbineCrudFactory _turbineFactory = new();
    private readonly LocalBatteryCrudFactory _batteryFactory = new();
    private readonly CentralBankCrudFactory _cbFactory = new();
    private readonly ForecastCrudFactory _forecastFactory = new();
    private readonly AccountStatementCrudFactory _statementFactory = new();
    private readonly FlushCrudFactory _flushFactory = new();
    private readonly MaintenanceCrudFactory _maintenanceFactory = new();
    private readonly PriceCrudFactory _priceFactory = new();
    private readonly TaxCrudFactory _taxFactory = new();
    private readonly EnergyGenerationLogCrudFactory _genLogFactory = new();
    private readonly UserCrudFactory _userFactory = new();
    private readonly CommercialDistributionCrudFactory _distFactory = new();

    public void SeedAllDevData()
    {
        try
        {
            Console.WriteLine("[SEEDER] Iniciando siembra de datos funcionales en tiempo real para todos los perfiles...");
            var now = TimeHelper.NowCR();

            SeedCentralBank(now);
            SeedPricesAndTaxes(now);
            SeedTurbinesAndBatteries(now);
            SeedBuyerData(now);
            new UserManager().SeedDevUsers();
            SeedOperationsData(now);

            Console.WriteLine("[SEEDER] ¡Siembra de datos finalizada satisfactoriamente!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER-ERROR] Error durante la siembra automática: {ex.Message}");
        }
    }

    private void SeedCentralBank(DateTime now)
    {
        try
        {
            var cb = _cbFactory.RetrieveSingleton();
            if (cb != null)
            {
                if (cb.CurrentInventory == 0m)
                {
                    _cbFactory.UpdateInventory(84500.50m, now);
                }
                if (cb.AutomaticCapacity == 0m)
                {
                    _cbFactory.UpdateAutomaticCapacity(150000m, now);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER] Error en Banco Central: {ex.Message}");
        }
    }

    private void SeedPricesAndTaxes(DateTime now)
    {
        try
        {
            var prices = _priceFactory.RetrieveAll<Price>();
            if (prices.Count == 0)
            {
                _priceFactory.Create(new Price
                {
                    PriceCRCPerMWh = 125500m,
                    ValidFrom = now.AddMonths(-6),
                    IsActive = true,
                    Created = now
                });
            }

            var taxes = _taxFactory.RetrieveAll<Tax>();
            if (taxes.Count == 0)
            {
                _taxFactory.Create(new Tax
                {
                    Name = "IVA Energía",
                    Percentage = 13m,
                    IsActive = true,
                    Created = now
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER] Error en Precios/Impuestos: {ex.Message}");
        }
    }

    private void SeedTurbinesAndBatteries(DateTime now)
    {
        try
        {
            var existing = _turbineFactory.RetrieveAll<Turbine>();
            if (existing.Count >= 4) return;

            var turbinesToSeed = new List<Turbine>
            {
                new Turbine { UniqueCode = "TURB-GUA-01", Name = "Parque Eólico Guanacaste I", Location = "Guanacaste, CR", Brand = "Vestas", Model = "V150-4.2 MW", Year = 2022, WeeklyNominalCapacity = 25000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-10), Created = now.AddMonths(-12) },
                new Turbine { UniqueCode = "TURB-GUA-02", Name = "Parque Eólico Guanacaste II", Location = "Guanacaste, CR", Brand = "Vestas", Model = "V150-4.2 MW", Year = 2023, WeeklyNominalCapacity = 30000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-5), Created = now.AddMonths(-6) },
                new Turbine { UniqueCode = "TURB-SOL-01", Name = "Planta Solar Miravalles", Location = "Alajuela, CR", Brand = "SunPower", Model = "Maxeon 5", Year = 2021, WeeklyNominalCapacity = 15000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-15), Created = now.AddMonths(-18) },
                new Turbine { UniqueCode = "TURB-HID-01", Name = "Hidroeléctrica Arenal", Location = "Guanacaste, CR", Brand = "GE Hydro", Model = "Francis 50", Year = 2018, WeeklyNominalCapacity = 45000m, Status = TurbineStates.UnderMaintenance, LastStateChange = now.AddDays(-2), Created = now.AddMonths(-24) },
                new Turbine { UniqueCode = "TURB-EOL-03", Name = "Parque Eólico Tilarán", Location = "Guanacaste, CR", Brand = "Gamesa", Model = "G114-2.5 MW", Year = 2020, WeeklyNominalCapacity = 20000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-7), Created = now.AddMonths(-15) }
            };

            foreach (var t in turbinesToSeed)
            {
                if (existing.Any(e => e.UniqueCode == t.UniqueCode)) continue;
                
                _turbineFactory.Create(t);
            }

            var updatedTurbines = _turbineFactory.RetrieveAll<Turbine>();
            foreach (var t in updatedTurbines)
            {
                var bat = _batteryFactory.RetrieveByTurbine(t.Id);
                if (bat == null)
                {
                    _batteryFactory.Create(new LocalBattery
                    {
                        TurbineId = t.Id,
                        StoredEnergy = 12500.50m,
                        Created = now
                    });
                }

                // Generar logs históricos iniciales para gráficos
                for (int i = 5; i >= 1; i--)
                {
                    _genLogFactory.Create(new EnergyGenerationLog
                    {
                        TurbineId = t.Id,
                        ActiveTimeSeconds = 3600m,
                        GeneratedEnergy = Math.Round(t.WeeklyNominalCapacity / 168m, 2),
                        EventDate = now.AddHours(-i * 4),
                        Created = now.AddHours(-i * 4)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER] Error en Turbinas/Baterías: {ex.Message}");
        }
    }

    private void SeedBuyerData(DateTime now)
    {
        try
        {
            var buyer = _userFactory.RetrieveByEmail("buyer@segede.local");
            if (buyer == null) return;

            var existingForecasts = _forecastFactory.RetrieveByBuyer(buyer.Id);
            if (existingForecasts.Count == 0)
            {
                _forecastFactory.Create(new Forecast
                {
                    BuyerId = buyer.Id,
                    Month = now.Month,
                    Year = now.Year,
                    AmountMWh = 14500m,
                    Status = ForecastStates.Distributed,
                    Created = now.AddDays(-15)
                });

                _forecastFactory.Create(new Forecast
                {
                    BuyerId = buyer.Id,
                    Month = now.AddMonths(1).Month,
                    Year = now.AddMonths(1).Year,
                    AmountMWh = 18200m,
                    Status = ForecastStates.Pending,
                    Created = now.AddDays(-5)
                });
            }

            var dists = _distFactory.RetrieveAll<CommercialDistribution>();
            if (dists.Count == 0)
            {
                _distFactory.Create(new CommercialDistribution
                {
                    Month = now.Month,
                    Year = now.Year,
                    ExecutionDate = now.AddDays(-2),
                    AvailableInventory = 25000m,
                    TotalDemand = 14500m,
                    DistributedEnergy = 14500m,
                    RoundingResidual = 0m,
                    Scenario = "Normal",
                    Created = now.AddDays(-2)
                });
                dists = _distFactory.RetrieveAll<CommercialDistribution>();
            }

            var forecasts = _forecastFactory.RetrieveAll<Forecast>();
            int validDistId = dists.Count > 0 ? dists[0].Id : 1;
            int validForeId = forecasts.Count > 0 ? forecasts[0].Id : 1;

            var existingStatements = _statementFactory.RetrieveByBuyer(buyer.Id);
            if (existingStatements.Count == 0)
            {
                _statementFactory.Create(new AccountStatement
                {
                    BuyerId = buyer.Id,
                    DistributionId = validDistId,
                    ForecastId = validForeId,
                    Month = now.AddMonths(-1).Month,
                    Year = now.Year,
                    AssignedMWh = 13800m,
                    UnitPrice = 125500m,
                    TaxPercentage = 13m,
                    Subtotal = 1731900000m,
                    TaxAmount = 225147000m,
                    Total = 1957047000m,
                    Status = StatementStates.Issued,
                    RevisionNumber = 1,
                    IssueDate = now.AddDays(-20),
                    Created = now.AddDays(-20)
                });

                _statementFactory.Create(new AccountStatement
                {
                    BuyerId = buyer.Id,
                    DistributionId = validDistId,
                    ForecastId = validForeId,
                    Month = now.Month,
                    Year = now.Year,
                    AssignedMWh = 14500m,
                    UnitPrice = 125500m,
                    TaxPercentage = 13m,
                    Subtotal = 1819750000m,
                    TaxAmount = 236567500m,
                    Total = 2056317500m,
                    Status = StatementStates.Issued,
                    RevisionNumber = 1,
                    IssueDate = now.AddDays(-2),
                    Created = now.AddDays(-2)
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER] Error en Datos de Comprador: {ex.Message}");
        }
    }

    private void SeedOperationsData(DateTime now)
    {
        try
        {
            var flushes = _flushFactory.RetrieveAll<Flush>();
            if (flushes.Count == 0)
            {
                _flushFactory.Create(new Flush
                {
                    ExecutionType = "Scheduled",
                    Status = FlushStates.Completed,
                    UserId = 1,
                    TotalTransferredEnergy = 24500.75m,
                    SaturationLoss = 120.50m,
                    StartDate = now.AddDays(-2).AddHours(-1),
                    EndDate = now.AddDays(-2),
                    Created = now.AddDays(-2)
                });
            }

            var maintenances = _maintenanceFactory.RetrieveAll<Maintenance>();
            if (maintenances.Count == 0)
            {
                var turbines = _turbineFactory.RetrieveAll<Turbine>();
                var t4 = turbines.FirstOrDefault(t => t.UniqueCode == "TURB-HID-01") ?? turbines.FirstOrDefault();
                if (t4 != null)
                {
                    _maintenanceFactory.Create(new Maintenance
                    {
                        TurbineId = t4.Id,
                        MaintenanceType = "Corrective",
                        EstimatedStartDate = now.AddDays(-1),
                        EstimatedEndDate = now.AddDays(3),
                        Status = MaintenanceStates.InProgress,
                        Created = now.AddDays(-1)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER] Error en Mantenimientos/Flushes: {ex.Message}");
        }
    }
}
