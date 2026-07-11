using SEGEDE_Grupo1.DataAccess.CRUD;
using SEGEDE_Grupo1.EntitiesDTOs.Constants;
using SEGEDE_Grupo1.EntitiesDTOs.Entities;
using SEGEDE_Grupo1.EntitiesDTOs.Helpers;
using SEGEDE_Grupo1.CoreApp.Helpers;

namespace SEGEDE_Grupo1.CoreApp.Managers;

// Manager encargado de poblar automáticamente y generar datos históricos extensos (5 años) para la base de datos (§14.13, RN-030).
// Inyecta datos reales de plantas eléctricas de Costa Rica, 5 clientes compradores principales con historial desde 2021, facturación, flushes y bitácoras.
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
    private readonly DistributionDetailCrudFactory _detailFactory = new();
    private readonly FailureCrudFactory _failureFactory = new();
    private readonly FlushSnapshotCrudFactory _snapshotFactory = new();
    private readonly CentralBankLogCrudFactory _cbLogFactory = new();
    private readonly FlushConfigCrudFactory _flushConfigFactory = new();

    public void SeedAllDevData()
    {
        try
        {
            Console.WriteLine("[SEEDER] Iniciando inyección y siembra de datos históricos reales (5 años) en base de datos...");
            var now = TimeHelper.NowCR();

            // 1. Usuarios e instituciones cliente (ICE, CNFL, JASEC, ESPH, COOPELESCA)
            new UserManager().SeedDevUsers();

            // 2. Banco Central, Configuración de Flush, Precios e Impuestos históricos
            SeedCentralBank(now);
            SeedFlushConfig(now);
            SeedPricesAndTaxes(now);

            // 3. Turbinas reales de Costa Rica, baterías y log de generación
            SeedTurbinesAndBatteries(now);

            // 4. Histórico comercial de 5 años (pronósticos, distribuciones, detalles y estados de cuenta) para todos los compradores
            SeedHistoricalBuyerData(now);

            // 5. Flushes operacionales, averías y mantenimientos de los últimos 5 años
            SeedOperationsData(now);

            Console.WriteLine("[SEEDER] ¡Inyección de datos reales finalizada exitosamente! Todos los paneles y tablas tienen información histórica de 2021 a 2026.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER-ERROR] Error durante la inyección de datos históricos: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void SeedCentralBank(DateTime now)
    {
        try
        {
            var cb = _cbFactory.RetrieveSingleton();
            if (cb == null)
            {
                _cbFactory.InitializeSingleton(285400.50m, 450000.00m, new DateTime(2021, 1, 1, 0, 0, 0));
                cb = _cbFactory.RetrieveSingleton();
            }
            else
            {
                if (cb.CurrentInventory < 1000m)
                {
                    _cbFactory.UpdateInventory(285400.50m, now);
                }
                if (cb.AutomaticCapacity < 1000m)
                {
                    _cbFactory.UpdateAutomaticCapacity(450000.00m, now);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER] Error en Banco Central: {ex.Message}");
        }
    }

    private void SeedFlushConfig(DateTime now)
    {
        try
        {
            var cfg = _flushConfigFactory.RetrieveSingleton();
            if (cfg == null)
            {
                _flushConfigFactory.InitializeSingleton(new TimeSpan(0, 0, 0), true, new DateTime(2021, 1, 1, 0, 0, 0));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER] Error en Configuración de Flush: {ex.Message}");
        }
    }

    private void SeedPricesAndTaxes(DateTime now)
    {
        try
        {
            var prices = _priceFactory.RetrieveAll<Price>();
            var historicalPrices = new[]
            {
                new { Price = 110000m, Year = 2021, Month = 1 },
                new { Price = 114500m, Year = 2022, Month = 1 },
                new { Price = 118000m, Year = 2023, Month = 1 },
                new { Price = 121500m, Year = 2024, Month = 1 },
                new { Price = 124000m, Year = 2025, Month = 1 },
                new { Price = 125500m, Year = 2026, Month = 1 }
            };

            foreach (var hp in historicalPrices)
            {
                if (!prices.Any(p => p.PriceCRCPerMWh == hp.Price))
                {
                    _priceFactory.Create(new Price
                    {
                        PriceCRCPerMWh = hp.Price,
                        ValidFrom = new DateTime(hp.Year, hp.Month, 1),
                        IsActive = (hp.Year == 2026),
                        Created = new DateTime(hp.Year, hp.Month, 1)
                    });
                }
            }

            var taxes = _taxFactory.RetrieveAll<Tax>();
            if (!taxes.Any(t => t.Name == "IVA Energía"))
            {
                _taxFactory.Create(new Tax
                {
                    Name = "IVA Energía",
                    Percentage = 0.13m,
                    ValidFrom = new DateTime(2021, 1, 1),
                    IsActive = true,
                    Created = new DateTime(2021, 1, 1)
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
            var existingTurbines = _turbineFactory.RetrieveAll<Turbine>();
            var realPowerPlants = new List<Turbine>
            {
                new Turbine { UniqueCode = "TURB-GUA-01", Name = "Parque Eólico Guanacaste I", Location = "Guanacaste, CR", Brand = "Vestas", Model = "V150-4.2 MW", Year = 2021, WeeklyNominalCapacity = 35000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-10), Created = new DateTime(2021, 2, 1) },
                new Turbine { UniqueCode = "TURB-GUA-02", Name = "Parque Eólico Guanacaste II", Location = "Guanacaste, CR", Brand = "Vestas", Model = "V150-4.2 MW", Year = 2022, WeeklyNominalCapacity = 40000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-5), Created = new DateTime(2022, 3, 15) },
                new Turbine { UniqueCode = "TURB-SOL-01", Name = "Planta Solar Miravalles", Location = "Alajuela, CR", Brand = "SunPower", Model = "Maxeon 5", Year = 2020, WeeklyNominalCapacity = 25000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-15), Created = new DateTime(2021, 1, 20) },
                new Turbine { UniqueCode = "TURB-HID-01", Name = "Hidroeléctrica Arenal", Location = "Guanacaste, CR", Brand = "GE Hydro", Model = "Francis 50", Year = 2018, WeeklyNominalCapacity = 65000m, Status = TurbineStates.UnderMaintenance, LastStateChange = now.AddDays(-2), Created = new DateTime(2021, 1, 10) },
                new Turbine { UniqueCode = "TURB-HID-02", Name = "Hidroeléctrica Reventazón", Location = "Limón, CR", Brand = "Andritz", Model = "Francis 73", Year = 2019, WeeklyNominalCapacity = 85000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-20), Created = new DateTime(2021, 1, 15) },
                new Turbine { UniqueCode = "TURB-EOL-03", Name = "Parque Eólico Tilarán", Location = "Guanacaste, CR", Brand = "Gamesa", Model = "G114-2.5 MW", Year = 2020, WeeklyNominalCapacity = 28000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-7), Created = new DateTime(2021, 4, 10) },
                new Turbine { UniqueCode = "TURB-EOL-04", Name = "Parque Eólico Chiripa", Location = "Guanacaste, CR", Brand = "Enercon", Model = "E-82", Year = 2020, WeeklyNominalCapacity = 32000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-12), Created = new DateTime(2021, 5, 20) },
                new Turbine { UniqueCode = "TURB-SOL-02", Name = "Planta Solar Juanilama", Location = "Guanacaste, CR", Brand = "Jinko Solar", Model = "Tiger Pro", Year = 2022, WeeklyNominalCapacity = 22000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-3), Created = new DateTime(2022, 6, 1) },
                new Turbine { UniqueCode = "TURB-HID-03", Name = "Hidroeléctrica Cachí", Location = "Cartago, CR", Brand = "Siemens", Model = "Pelton 40", Year = 2017, WeeklyNominalCapacity = 50000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-18), Created = new DateTime(2021, 3, 10) },
                new Turbine { UniqueCode = "TURB-EOL-05", Name = "Parque Eólico Tejona", Location = "Guanacaste, CR", Brand = "Windworld", Model = "W420", Year = 2018, WeeklyNominalCapacity = 24000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-25), Created = new DateTime(2021, 2, 18) },
                new Turbine { UniqueCode = "TURB-HID-04", Name = "Hidroeléctrica Pirrís", Location = "San José, CR", Brand = "GE Hydro", Model = "Francis 60", Year = 2019, WeeklyNominalCapacity = 60000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-8), Created = new DateTime(2021, 1, 25) },
                new Turbine { UniqueCode = "TURB-SOL-03", Name = "Parque Solar Valle del Sol", Location = "Puntarenas, CR", Brand = "Trina Solar", Model = "Vertex", Year = 2023, WeeklyNominalCapacity = 30000m, Status = TurbineStates.Active, LastStateChange = now.AddDays(-4), Created = new DateTime(2023, 2, 10) }
            };

            foreach (var t in realPowerPlants)
            {
                var existing = existingTurbines.FirstOrDefault(e => e.UniqueCode == t.UniqueCode);
                if (existing == null)
                {
                    _turbineFactory.Create(t);
                }
                else if (existing.Name.StartsWith("Turbina "))
                {
                    existing.Name = t.Name;
                    existing.Location = t.Location;
                    existing.Brand = t.Brand;
                    existing.Model = t.Model;
                    existing.WeeklyNominalCapacity = t.WeeklyNominalCapacity;
                    _turbineFactory.Update(existing);
                }
            }

            var allTurbines = _turbineFactory.RetrieveAll<Turbine>();
            var allBatteries = _batteryFactory.RetrieveAll<LocalBattery>();

            var rand = new Random(12345);
            foreach (var t in allTurbines)
            {
                var bat = allBatteries.FirstOrDefault(b => b.TurbineId == t.Id);
                if (bat == null)
                {
                    _batteryFactory.Create(new LocalBattery
                    {
                        TurbineId = t.Id,
                        StoredEnergy = Math.Round(t.WeeklyNominalCapacity * 0.65m, 2),
                        Created = t.Created
                    });
                }

                // Si la turbina tiene pocos logs, generar historial mensual desde 2021 a 2026
                int logsCount = _genLogFactory.RetrieveByTurbine(t.Id).Count;
                if (logsCount < 12)
                {
                    for (int year = 2021; year <= 2026; year++)
                    {
                        int maxMonth = (year == 2026) ? now.Month : 12;
                        for (int month = 1; month <= maxMonth; month++)
                        {
                            var logDate = new DateTime(year, month, Math.Min(15, DateTime.DaysInMonth(year, month)), 12, 0, 0);
                            // Factor estacional y variación realista
                            decimal seasonalFactor = (month >= 5 && month <= 11) ? 1.15m : 0.90m;
                            decimal generated = Math.Round((t.WeeklyNominalCapacity * 4.3m) * seasonalFactor * ((decimal)rand.Next(85, 105) / 100m), 2);

                            _genLogFactory.Create(new EnergyGenerationLog
                            {
                                TurbineId = t.Id,
                                ActiveTimeSeconds = 2592000m, // ~30 días en segundos
                                GeneratedEnergy = generated,
                                EventDate = logDate,
                                Created = logDate
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER] Error en Turbinas/Baterías: {ex.Message}");
        }
    }

    private void SeedHistoricalBuyerData(DateTime now)
    {
        try
        {
            var allUsers = _userFactory.RetrieveAll<User>();
            var buyers = allUsers.Where(u => string.Equals(u.Role, "Buyer", StringComparison.OrdinalIgnoreCase)).ToList();
            if (buyers.Count == 0) return;

            var allDists = _distFactory.RetrieveAll<CommercialDistribution>();
            var allForecasts = _forecastFactory.RetrieveAll<Forecast>();
            var allStatements = _statementFactory.RetrieveAll<AccountStatement>();

            var rand = new Random(54321);

            for (int year = 2021; year <= 2026; year++)
            {
                int maxMonth = (year == 2026) ? now.Month : 12;
                for (int month = 1; month <= maxMonth; month++)
                {
                    var distDate = new DateTime(year, month, Math.Min(28, DateTime.DaysInMonth(year, month)), 10, 0, 0);
                    
                    // 1. Verificar o crear Comercial Distribution del mes/año
                    var dist = allDists.FirstOrDefault(d => d.Month == month && d.Year == year);
                    if (dist == null)
                    {
                        decimal baseInventory = (month >= 5 && month <= 11) ? 220000m : 160000m;
                        decimal inventory = Math.Round(baseInventory * ((decimal)rand.Next(95, 110) / 100m), 2);
                        
                        dist = new CommercialDistribution
                        {
                            Month = month,
                            Year = year,
                            ExecutionDate = distDate,
                            AvailableInventory = inventory,
                            TotalDemand = Math.Round(inventory * 0.85m, 2),
                            DistributedEnergy = Math.Round(inventory * 0.85m, 2),
                            RoundingResidual = 0m,
                            Scenario = (inventory > 180000m) ? "Superávit" : "Normal",
                            Created = distDate
                        };
                        _distFactory.Create(dist);
                        allDists = _distFactory.RetrieveAll<CommercialDistribution>();
                        dist = allDists.FirstOrDefault(d => d.Month == month && d.Year == year);
                    }

                    if (dist == null) continue;

                    // Detalle de distribución para esta distribución
                    var distDetails = _detailFactory.RetrieveByDistribution(dist.Id);

                    // Precio unitario por año
                    decimal unitPrice = year switch
                    {
                        2021 => 110000m,
                        2022 => 114500m,
                        2023 => 118000m,
                        2024 => 121500m,
                        2025 => 124000m,
                        _ => 125500m
                    };

                    // 2. Para cada cliente comprador, verificar o crear Forecast, Detalle y Estado de Cuenta (Factura)
                    foreach (var b in buyers)
                    {
                        // Pronóstico de demanda
                        var forecast = allForecasts.FirstOrDefault(f => f.BuyerId == b.Id && f.Month == month && f.Year == year);
                        if (forecast == null)
                        {
                            decimal baseDemand = b.Email.Contains("ice@") ? 45000m :
                                                 b.Email.Contains("cnfl@") ? 35000m :
                                                 b.Email.Contains("jasec@") ? 18000m :
                                                 b.Email.Contains("esph@") ? 20000m :
                                                 b.Email.Contains("coopelesca@") ? 15000m : 14000m;

                            decimal demand = Math.Round(baseDemand * ((decimal)rand.Next(90, 110) / 100m), 2);
                            string fStatus = (year == 2026 && month == now.Month) ? ForecastStates.Pending : ForecastStates.Distributed;

                            forecast = new Forecast
                            {
                                BuyerId = b.Id,
                                Month = month,
                                Year = year,
                                AmountMWh = demand,
                                Status = fStatus,
                                Created = new DateTime(year, month, 1)
                            };
                            _forecastFactory.Create(forecast);
                            allForecasts = _forecastFactory.RetrieveAll<Forecast>();
                            forecast = allForecasts.FirstOrDefault(f => f.BuyerId == b.Id && f.Month == month && f.Year == year);
                        }

                        if (forecast == null) continue;

                        // Detalle de distribución
                        var detail = distDetails.FirstOrDefault(dt => dt.BuyerId == b.Id);
                        if (detail == null)
                        {
                            decimal assigned = Math.Round(forecast.AmountMWh * 0.98m, 2);
                            detail = new DistributionDetail
                            {
                                DistributionId = dist.Id,
                                BuyerId = b.Id,
                                ForecastId = forecast.Id,
                                RequestedMWh = forecast.AmountMWh,
                                AssignedMWh = assigned,
                                UnsuppliedDemand = Math.Max(0m, forecast.AmountMWh - assigned),
                                Created = distDate
                            };
                            _detailFactory.Create(detail);
                        }

                        // Estado de Cuenta (Factura)
                        var statement = allStatements.FirstOrDefault(s => s.BuyerId == b.Id && s.Month == month && s.Year == year);
                        if (statement == null)
                        {
                            decimal assignedMWh = Math.Round(forecast.AmountMWh * 0.98m, 2);
                            decimal subtotal = Math.Round(assignedMWh * unitPrice, 2);
                            decimal taxAmount = Math.Round(subtotal * 0.13m, 2);
                            decimal total = subtotal + taxAmount;
                            string stStatus = (year == 2026 && month >= now.Month - 1) ? StatementStates.Issued : StatementStates.Paid;

                            statement = new AccountStatement
                            {
                                BuyerId = b.Id,
                                DistributionId = dist.Id,
                                ForecastId = forecast.Id,
                                Month = month,
                                Year = year,
                                AssignedMWh = assignedMWh,
                                UnitPrice = unitPrice,
                                TaxPercentage = 0.13m,
                                Subtotal = subtotal,
                                TaxAmount = taxAmount,
                                Total = total,
                                Status = stStatus,
                                RevisionNumber = 1,
                                IssueDate = distDate.AddDays(1),
                                Created = distDate.AddDays(1)
                            };
                            _statementFactory.Create(statement);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SEEDER] Error en Datos Históricos de Comprador: {ex.Message}");
        }
    }

    private void SeedOperationsData(DateTime now)
    {
        try
        {
            var flushes = _flushFactory.RetrieveAll<Flush>();
            var rand = new Random(999);

            if (flushes.Count < 12)
            {
                for (int year = 2021; year <= 2026; year++)
                {
                    int maxMonth = (year == 2026) ? now.Month : 12;
                    for (int month = 1; month <= maxMonth; month += 2) // Un flush cada 2 meses
                    {
                        var flushDate = new DateTime(year, month, 15, 14, 0, 0);
                        if (!flushes.Any(f => f.StartDate.Year == year && f.StartDate.Month == month))
                        {
                            decimal transferred = Math.Round((decimal)rand.Next(18000, 45000) + ((decimal)rand.Next(0, 99) / 100m), 2);
                            _flushFactory.Create(new Flush
                            {
                                ExecutionType = "Scheduled",
                                Status = FlushStates.Completed,
                                UserId = 1,
                                TotalTransferredEnergy = transferred,
                                SaturationLoss = Math.Round(transferred * 0.015m, 2),
                                StartDate = flushDate,
                                EndDate = flushDate.AddHours(2),
                                Created = flushDate
                            });
                        }
                    }
                }
            }

            var maintenances = _maintenanceFactory.RetrieveAll<Maintenance>();
            if (maintenances.Count < 5)
            {
                var turbines = _turbineFactory.RetrieveAll<Turbine>();
                var tMain = turbines.FirstOrDefault(t => t.UniqueCode == "TURB-HID-01") ?? turbines.FirstOrDefault();
                if (tMain != null)
                {
                    _maintenanceFactory.Create(new Maintenance
                    {
                        TurbineId = tMain.Id,
                        MaintenanceType = "Corrective",
                        EstimatedStartDate = now.AddDays(-2),
                        EstimatedEndDate = now.AddDays(5),
                        ActualStartDate = now.AddDays(-2),
                        Status = MaintenanceStates.InProgress,
                        Created = now.AddDays(-2)
                    });
                }

                var tPrev = turbines.FirstOrDefault(t => t.UniqueCode == "TURB-GUA-01") ?? turbines.LastOrDefault();
                if (tPrev != null && tPrev.Id != tMain?.Id)
                {
                    _maintenanceFactory.Create(new Maintenance
                    {
                        TurbineId = tPrev.Id,
                        MaintenanceType = "Preventive",
                        EstimatedStartDate = now.AddDays(-30),
                        EstimatedEndDate = now.AddDays(-28),
                        ActualStartDate = now.AddDays(-30),
                        ActualEndDate = now.AddDays(-28),
                        Result = "Cambio exitoso de rodamientos y lubricación del sistema.",
                        Status = MaintenanceStates.Completed,
                        Created = now.AddDays(-30)
                    });
                }
            }

            var failures = _failureFactory.RetrieveAll<Failure>();
            if (failures.Count < 4)
            {
                var turbines = _turbineFactory.RetrieveAll<Turbine>();
                foreach (var t in turbines.Take(4))
                {
                    _failureFactory.Create(new Failure
                    {
                        TurbineId = t.Id,
                        FailureDate = now.AddMonths(-rand.Next(1, 24)),
                        Description = "Oscilación transitoria de voltaje en subestación de interconexión.",
                        Severity = "Medium",
                        Created = now.AddMonths(-rand.Next(1, 24))
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
