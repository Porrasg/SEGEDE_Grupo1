/* ============================================================================
   SGDE - Stored Procedures
   Generated from the exact names and parameters invoked by the CrudFactory
   classes under DataAccess/CRUD, via Operation.ProcedureName + SqlParameter (by name).
   Rules applied:
     - SPs that "increment" a counter (FailedAttempts, ResendCount) do not
       receive the new value as a parameter: they compute it themselves (+1).
     - CRE_PRICE_PR and CRE_TAX_PR automatically close the previously active
       row before inserting the new one (per the comments in
       BillingManager.SetPrice/SetTax: "handled by the insert SP").
     - Paged listing SPs use OFFSET/FETCH; no double round-trip needed since
       the C# layer computes TotalPages from a separate count when required.
   ============================================================================ */


/* ============================================================================
   USERS
   ============================================================================ */
GO
CREATE PROCEDURE CRE_USER_PR
    @Identification VARCHAR(12), @FirstName VARCHAR(150), @LastName VARCHAR(150),
    @BirthDate DATE, @Phone VARCHAR(20), @Email VARCHAR(250), @PhotoUrl VARCHAR(MAX) = NULL,
    @PasswordHash VARCHAR(512), @Role VARCHAR(30), @Status VARCHAR(30), @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblUsers (Identification, FirstName, LastName, BirthDate, Phone, Email, PhotoUrl, PasswordHash, Role, Status, FailedAttempts, Created)
    VALUES (@Identification, @FirstName, @LastName, @BirthDate, @Phone, @Email, @PhotoUrl, @PasswordHash, @Role, @Status, 0, @Created);
END
GO
CREATE PROCEDURE UPD_USER_PR
    @Id INT, @FirstName VARCHAR(150), @LastName VARCHAR(150), @Phone VARCHAR(20),
    @Role VARCHAR(30), @Status VARCHAR(30), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblUsers SET FirstName=@FirstName, LastName=@LastName, Phone=@Phone, Role=@Role, Status=@Status, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE DEL_USER_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM tblUsers WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ID_USER_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblUsers WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_USER_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblUsers ORDER BY Id;
END
GO
CREATE PROCEDURE RET_EMAIL_USER_PR @Email VARCHAR(250)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblUsers WHERE Email=@Email;
END
GO
CREATE PROCEDURE UPD_STATUS_USER_PR @Id INT, @Status VARCHAR(30), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblUsers SET Status=@Status, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE UPD_ATTEMPTS_USER_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblUsers SET FailedAttempts = FailedAttempts + 1 WHERE Id=@Id;
END
GO
CREATE PROCEDURE UPD_RESET_ATTEMPTS_USER_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblUsers SET FailedAttempts = 0 WHERE Id=@Id;
END
GO
CREATE PROCEDURE UPD_BLOCK_USER_PR @Id INT, @BlockedAt DATETIME2, @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblUsers SET Status='Blocked', BlockedAt=@BlockedAt, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE UPD_PROFILE_USER_PR
    @Id INT, @Phone VARCHAR(20), @PhotoUrl VARCHAR(MAX) = NULL, @PasswordHash VARCHAR(512) = NULL, @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblUsers
    SET Phone=@Phone,
        PhotoUrl = ISNULL(@PhotoUrl, PhotoUrl),
        PasswordHash = ISNULL(@PasswordHash, PasswordHash),
        Updated=@Updated
    WHERE Id=@Id;
END
GO
CREATE PROCEDURE UPD_PASSWORD_USER_PR @Id INT, @PasswordHash VARCHAR(512), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblUsers SET PasswordHash=@PasswordHash, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_EXPIRED_BLOCKS_USER_PR @Threshold DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblUsers WHERE Status='Blocked' AND BlockedAt IS NOT NULL AND BlockedAt <= @Threshold;
END
GO


/* ============================================================================
   OTP ATTEMPTS
   ============================================================================ */
/* NOTA (2026-07-11): @OtpCode y @CodeExpiration son opcionales para mantener compatibilidad
   entre ramas del equipo: la rama Yorzeth no los envía (el código vive en el servicio OTP
   externo, §13.3) y otra rama del equipo sí los persiste. Las columnas se agregaron a
   tblOtpAttempts directamente en la BD compartida por esa otra rama. */
CREATE PROCEDURE CRE_OTP_ATTEMPT_PR
    @UserId INT, @UsageType NVARCHAR(50), @OtpCode NVARCHAR(6) = NULL, @CodeExpiration DATETIME2 = NULL,
    @ResendCount INT, @FailedAttempts INT, @Status NVARCHAR(20),
    @StartDate DATETIME2, @WindowExpiration DATETIME2, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblOtpAttempts (UserId, UsageType, OtpCode, CodeExpiration, ResendCount, FailedAttempts, Status, StartDate, WindowExpiration, Created)
    VALUES (@UserId, @UsageType, @OtpCode, @CodeExpiration, @ResendCount, @FailedAttempts, @Status, @StartDate, @WindowExpiration, @Created);
END
GO
CREATE PROCEDURE UPD_OTP_ATTEMPT_PR @Id INT, @ResendCount INT, @FailedAttempts INT, @Status VARCHAR(15), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblOtpAttempts SET ResendCount=@ResendCount, FailedAttempts=@FailedAttempts, Status=@Status, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ID_OTP_ATTEMPT_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblOtpAttempts WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ACTIVE_OTP_ATTEMPT_PR @UserId INT, @UsageType VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM tblOtpAttempts
    WHERE UserId=@UserId AND UsageType=@UsageType AND Status='InProgress'
    ORDER BY Created DESC;
END
GO
CREATE PROCEDURE UPD_RESEND_OTP_ATTEMPT_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblOtpAttempts SET ResendCount = ResendCount + 1 WHERE Id=@Id;
END
GO
CREATE PROCEDURE UPD_FAIL_OTP_ATTEMPT_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblOtpAttempts SET FailedAttempts = FailedAttempts + 1 WHERE Id=@Id;
END
GO
CREATE PROCEDURE UPD_STATUS_OTP_ATTEMPT_PR @Id INT, @Status VARCHAR(15), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblOtpAttempts SET Status=@Status, Updated=@Updated WHERE Id=@Id;
END
GO


/* ============================================================================
   TURBINES
   ============================================================================ */
CREATE PROCEDURE CRE_TURBINE_PR
    @UniqueCode VARCHAR(50), @Name VARCHAR(150), @Location VARCHAR(300), @Brand VARCHAR(100),
    @Model VARCHAR(100), @Year INT, @WeeklyNominalCapacity DECIMAL(18,4), @Status VARCHAR(40),
    @LastStateChange DATETIME2, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblTurbines (UniqueCode, Name, Location, Brand, Model, Year, WeeklyNominalCapacity, Status, LastStateChange, Created)
    VALUES (@UniqueCode, @Name, @Location, @Brand, @Model, @Year, @WeeklyNominalCapacity, @Status, @LastStateChange, @Created);
END
GO
CREATE PROCEDURE UPD_TURBINE_PR
    @Id INT, @Name VARCHAR(150), @Location VARCHAR(300), @Brand VARCHAR(100), @Model VARCHAR(100),
    @WeeklyNominalCapacity DECIMAL(18,4), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblTurbines SET Name=@Name, Location=@Location, Brand=@Brand, Model=@Model, WeeklyNominalCapacity=@WeeklyNominalCapacity, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE DEL_TURBINE_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM tblTurbines WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ID_TURBINE_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblTurbines WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_TURBINE_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblTurbines ORDER BY Id;
END
GO
CREATE PROCEDURE RET_ALL_ACTIVE_TURBINE_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblTurbines WHERE Status='Active' ORDER BY Id;
END
GO
CREATE PROCEDURE RET_BY_CODE_TURBINE_PR @UniqueCode VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblTurbines WHERE UniqueCode=@UniqueCode;
END
GO
CREATE PROCEDURE UPD_STATUS_TURBINE_PR
    @Id INT, @Status VARCHAR(40), @LastStateChange DATETIME2, @LastMaintenance DATETIME2 = NULL, @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblTurbines SET Status=@Status, LastStateChange=@LastStateChange, LastMaintenance=ISNULL(@LastMaintenance, LastMaintenance), Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_OVERDUE_TURBINE_PR @Threshold DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblTurbines
    WHERE (LastMaintenance IS NULL OR LastMaintenance <= @Threshold)
      AND Status NOT IN ('Decommissioned', 'SuspendedForNonCompliance');
END
GO
CREATE PROCEDURE UPD_MAINT_DATE_TURBINE_PR @Id INT, @LastMaintenance DATETIME2, @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblTurbines SET LastMaintenance=@LastMaintenance, Updated=@Updated WHERE Id=@Id;
END
GO


/* ============================================================================
   TURBINE STATE HISTORY (WORM)
   ============================================================================ */
CREATE PROCEDURE CRE_TRB_STATE_PR
    @TurbineId INT, @PreviousState VARCHAR(40), @NewState VARCHAR(40), @ChangeDate DATETIME2,
    @Reason VARCHAR(500), @UserId INT, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblTurbineStateHistory (TurbineId, PreviousState, NewState, ChangeDate, Reason, UserId, Created)
    VALUES (@TurbineId, @PreviousState, @NewState, @ChangeDate, @Reason, @UserId, @Created);
END
GO
CREATE PROCEDURE RET_ID_TRB_STATE_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblTurbineStateHistory WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_TRB_STATE_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblTurbineStateHistory ORDER BY Id;
END
GO
CREATE PROCEDURE RET_BY_TURBINE_TRB_STATE_PR @TurbineId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblTurbineStateHistory WHERE TurbineId=@TurbineId ORDER BY ChangeDate DESC;
END
GO


/* ============================================================================
   LOCAL BATTERY
   ============================================================================ */
CREATE PROCEDURE CRE_LOCAL_BAT_PR @TurbineId INT, @StoredEnergy DECIMAL(18,4), @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblLocalBattery (TurbineId, StoredEnergy, Created) VALUES (@TurbineId, @StoredEnergy, @Created);
END
GO
CREATE PROCEDURE RET_ID_LOCAL_BAT_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblLocalBattery WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_LOCAL_BAT_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblLocalBattery ORDER BY Id;
END
GO
CREATE PROCEDURE RET_BY_TURBINE_LOCAL_BAT_PR @TurbineId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblLocalBattery WHERE TurbineId=@TurbineId;
END
GO
CREATE PROCEDURE UPD_ENERGY_LOCAL_BAT_PR @Id INT, @StoredEnergy DECIMAL(18,4), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblLocalBattery SET StoredEnergy=@StoredEnergy, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_NONEMPTY_LOCAL_BAT_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblLocalBattery WHERE StoredEnergy > 0;
END
GO


/* ============================================================================
   MAINTENANCES
   ============================================================================ */
CREATE PROCEDURE CRE_MAINT_PR
    @TurbineId INT, @MaintenanceType VARCHAR(15), @EstimatedStartDate DATETIME2, @EstimatedEndDate DATETIME2,
    @Status VARCHAR(15), @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblMaintenances (TurbineId, MaintenanceType, EstimatedStartDate, EstimatedEndDate, Status, Created)
    VALUES (@TurbineId, @MaintenanceType, @EstimatedStartDate, @EstimatedEndDate, @Status, @Created);
END
GO
CREATE PROCEDURE UPD_MAINT_PR
    @Id INT, @ActualStartDate DATETIME2 = NULL, @ActualEndDate DATETIME2 = NULL, @Result VARCHAR(500) = NULL,
    @Status VARCHAR(15), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblMaintenances SET ActualStartDate=@ActualStartDate, ActualEndDate=@ActualEndDate, Result=@Result, Status=@Status, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE DEL_MAINT_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM tblMaintenances WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ID_MAINT_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblMaintenances WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_MAINT_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblMaintenances ORDER BY Id DESC;
END
GO
CREATE PROCEDURE RET_BY_TURBINE_MAINT_PR @TurbineId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblMaintenances WHERE TurbineId=@TurbineId ORDER BY Id DESC;
END
GO
CREATE PROCEDURE RET_ACTIVE_PREV_MAINT_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM tblMaintenances WHERE MaintenanceType='Preventive' AND Status IN ('Scheduled','InProgress') ORDER BY Id;
END
GO
CREATE PROCEDURE UPD_COMPLETE_MAINT_PR @Id INT, @ActualEndDate DATETIME2, @Result VARCHAR(500), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblMaintenances SET ActualEndDate=@ActualEndDate, Result=@Result, Status='Completed', Updated=@Updated WHERE Id=@Id;
END
GO


/* ============================================================================
   FAILURES
   ============================================================================ */
CREATE PROCEDURE CRE_FAILURE_PR
    @TurbineId INT, @FailureDate DATETIME2, @Description VARCHAR(1000), @Severity VARCHAR(10), @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblFailures (TurbineId, FailureDate, Description, Severity, Created)
    VALUES (@TurbineId, @FailureDate, @Description, @Severity, @Created);
END
GO
CREATE PROCEDURE RET_ID_FAILURE_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblFailures WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_FAILURE_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblFailures ORDER BY Id DESC;
END
GO
CREATE PROCEDURE RET_BY_TURBINE_FAILURE_PR @TurbineId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblFailures WHERE TurbineId=@TurbineId ORDER BY Id DESC;
END
GO
CREATE PROCEDURE RET_COUNT_BY_TURBINE_FAILURE_PR @TurbineId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS Count FROM tblFailures WHERE TurbineId=@TurbineId;
END
GO


/* ============================================================================
   ENERGY GENERATION LOG (WORM)
   ============================================================================ */
CREATE PROCEDURE CRE_EG_LOG_PR
    @TurbineId INT, @ActiveTimeSeconds DECIMAL(18,4), @GeneratedEnergy DECIMAL(18,4), @EventDate DATETIME2, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblEnergyGenerationLog (TurbineId, ActiveTimeSeconds, GeneratedEnergy, EventDate, Created)
    VALUES (@TurbineId, @ActiveTimeSeconds, @GeneratedEnergy, @EventDate, @Created);
END
GO
CREATE PROCEDURE RET_ID_EG_LOG_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblEnergyGenerationLog WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_BY_TURBINE_EG_LOG_PR @TurbineId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblEnergyGenerationLog WHERE TurbineId=@TurbineId ORDER BY EventDate DESC;
END
GO
CREATE PROCEDURE RET_PAGED_BY_TURBINE_EG_LOG_PR @TurbineId INT, @PageNumber INT, @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblEnergyGenerationLog WHERE TurbineId=@TurbineId
    ORDER BY EventDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
CREATE PROCEDURE RET_SUM_BY_TURBINE_EG_LOG_PR @TurbineId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(SUM(ActiveTimeSeconds),0) AS TotalActiveSeconds, ISNULL(SUM(GeneratedEnergy),0) AS TotalGeneratedEnergy
    FROM tblEnergyGenerationLog WHERE TurbineId=@TurbineId;
END
GO


/* ============================================================================
   ENERGY LOSS LOG (WORM)
   ============================================================================ */
CREATE PROCEDURE CRE_EL_LOG_PR
    @TurbineId INT, @InactiveTimeSeconds DECIMAL(18,4), @LostEnergy DECIMAL(18,4), @Cause VARCHAR(25), @EventDate DATETIME2, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblEnergyLossLog (TurbineId, InactiveTimeSeconds, LostEnergy, Cause, EventDate, Created)
    VALUES (@TurbineId, @InactiveTimeSeconds, @LostEnergy, @Cause, @EventDate, @Created);
END
GO
CREATE PROCEDURE RET_ID_EL_LOG_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblEnergyLossLog WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_BY_TURBINE_EL_LOG_PR @TurbineId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblEnergyLossLog WHERE TurbineId=@TurbineId ORDER BY EventDate DESC;
END
GO
CREATE PROCEDURE RET_BY_CAUSE_EL_LOG_PR @TurbineId INT, @Cause VARCHAR(25)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblEnergyLossLog WHERE TurbineId=@TurbineId AND Cause=@Cause ORDER BY EventDate DESC;
END
GO
CREATE PROCEDURE RET_SUM_BY_TURBINE_EL_LOG_PR @TurbineId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(SUM(LostEnergy),0) AS TotalLostEnergy FROM tblEnergyLossLog WHERE TurbineId=@TurbineId;
END
GO


/* ============================================================================
   FLUSH CONFIG (SINGLETON)
   ============================================================================ */
CREATE PROCEDURE RET_SINGLETON_FLUSH_CFG_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblFlushConfig WHERE Id=1;
END
GO
CREATE PROCEDURE INIT_FLUSH_CFG_PR @ExecutionTime TIME, @IsAutomatic BIT, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM tblFlushConfig WHERE Id=1)
        INSERT INTO tblFlushConfig (Id, ExecutionTime, IsAutomatic, Created)
        VALUES (1, @ExecutionTime, @IsAutomatic, @Created);
END
GO
CREATE PROCEDURE UPD_SINGLETON_FLUSH_CFG_PR @ExecutionTime TIME, @IsAutomatic BIT, @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblFlushConfig SET ExecutionTime=@ExecutionTime, IsAutomatic=@IsAutomatic, Updated=@Updated WHERE Id=1;
END
GO


/* ============================================================================
   CENTRAL BANK (SINGLETON)
   ============================================================================ */
CREATE PROCEDURE RET_SINGLETON_CENT_BANK_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCentralBank WHERE Id=1;
END
GO
CREATE PROCEDURE INIT_CENT_BANK_PR @CurrentInventory DECIMAL(18,4), @AutomaticCapacity DECIMAL(18,4), @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM tblCentralBank WHERE Id=1)
        INSERT INTO tblCentralBank (Id, CurrentInventory, AutomaticCapacity, Created)
        VALUES (1, @CurrentInventory, @AutomaticCapacity, @Created);
END
GO
CREATE PROCEDURE UPD_INVENTORY_CENT_BANK_PR @CurrentInventory DECIMAL(18,4), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblCentralBank SET CurrentInventory=@CurrentInventory, Updated=@Updated WHERE Id=1;
END
GO
CREATE PROCEDURE UPD_AUTO_CAP_CENT_BANK_PR @AutomaticCapacity DECIMAL(18,4), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblCentralBank SET AutomaticCapacity=@AutomaticCapacity, Updated=@Updated WHERE Id=1;
END
GO
CREATE PROCEDURE UPD_MANUAL_CAP_CENT_BANK_PR @ManualCapacity DECIMAL(18,4) = NULL, @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblCentralBank SET ManualCapacity=@ManualCapacity, Updated=@Updated WHERE Id=1;
END
GO


/* ============================================================================
   FLUSH
   ============================================================================ */
CREATE PROCEDURE CRE_FLUSH_PR
    @ExecutionType VARCHAR(12), @Status VARCHAR(15), @UserId INT = NULL, @TotalTransferredEnergy DECIMAL(18,4),
    @SaturationLoss DECIMAL(18,4), @StartDate DATETIME2, @EndDate DATETIME2 = NULL, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblFlush (ExecutionType, Status, UserId, TotalTransferredEnergy, SaturationLoss, StartDate, EndDate, Created)
    VALUES (@ExecutionType, @Status, @UserId, @TotalTransferredEnergy, @SaturationLoss, @StartDate, @EndDate, @Created);
END
GO
CREATE PROCEDURE RET_ID_FLUSH_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblFlush WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_FLUSH_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblFlush ORDER BY Id DESC;
END
GO
CREATE PROCEDURE RET_ACTIVE_FLUSH_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM tblFlush WHERE Status='Processing' ORDER BY Id DESC;
END
GO
CREATE PROCEDURE UPD_STATUS_FLUSH_PR
    @Id INT, @Status VARCHAR(15), @EndDate DATETIME2 = NULL, @TotalTransferredEnergy DECIMAL(18,4), @SaturationLoss DECIMAL(18,4), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblFlush SET Status=@Status, EndDate=@EndDate, TotalTransferredEnergy=@TotalTransferredEnergy, SaturationLoss=@SaturationLoss, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_PAGED_FLUSH_PR @PageNumber INT, @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblFlush ORDER BY Id DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO


/* ============================================================================
   FLUSH SNAPSHOT (WORM)
   ============================================================================ */
CREATE PROCEDURE CRE_FLUSH_SNAP_PR
    @FlushId INT, @TurbineId INT, @LocalBatteryId INT, @CapturedEnergy DECIMAL(18,4), @EventDate DATETIME2, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblFlushSnapshot (FlushId, TurbineId, LocalBatteryId, CapturedEnergy, EventDate, Created)
    VALUES (@FlushId, @TurbineId, @LocalBatteryId, @CapturedEnergy, @EventDate, @Created);
END
GO
CREATE PROCEDURE RET_ID_FLUSH_SNAP_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblFlushSnapshot WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_BY_FLUSH_SNAP_PR @FlushId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblFlushSnapshot WHERE FlushId=@FlushId;
END
GO
CREATE PROCEDURE RET_BY_TURBINE_SNAP_PR @TurbineId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblFlushSnapshot WHERE TurbineId=@TurbineId ORDER BY EventDate DESC;
END
GO


/* ============================================================================
   SATURATION LOG (WORM)
   ============================================================================ */
CREATE PROCEDURE CRE_SAT_LOG_PR
    @FlushId INT, @PreviousInventory DECIMAL(18,4), @NewInventory DECIMAL(18,4), @ExcessEnergy DECIMAL(18,4), @EventDate DATETIME2, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblSaturationLog (FlushId, PreviousInventory, NewInventory, ExcessEnergy, EventDate, Created)
    VALUES (@FlushId, @PreviousInventory, @NewInventory, @ExcessEnergy, @EventDate, @Created);
END
GO
CREATE PROCEDURE RET_ID_SAT_LOG_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblSaturationLog WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_BY_FLUSH_SAT_LOG_PR @FlushId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblSaturationLog WHERE FlushId=@FlushId;
END
GO


/* ============================================================================
   CENTRAL BANK LOG
   ============================================================================ */
CREATE PROCEDURE CRE_CB_LOG_PR
    @MovementType VARCHAR(15), @Amount DECIMAL(18,4), @ResultingInventory DECIMAL(18,4),
    @FlushId INT = NULL, @DistributionId INT = NULL, @EventDate DATETIME2, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblCentralBankLog (MovementType, Amount, ResultingInventory, FlushId, DistributionId, EventDate, Created)
    VALUES (@MovementType, @Amount, @ResultingInventory, @FlushId, @DistributionId, @EventDate, @Created);
END
GO
CREATE PROCEDURE RET_ID_CB_LOG_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCentralBankLog WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_CB_LOG_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCentralBankLog ORDER BY Id DESC;
END
GO
CREATE PROCEDURE RET_PAGED_CB_LOG_PR @PageNumber INT, @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCentralBankLog ORDER BY Id DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
CREATE PROCEDURE RET_BY_FLUSH_CB_LOG_PR @FlushId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCentralBankLog WHERE FlushId=@FlushId;
END
GO
CREATE PROCEDURE RET_BY_DIST_CB_LOG_PR @DistributionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCentralBankLog WHERE DistributionId=@DistributionId;
END
GO


/* ============================================================================
   FORECAST
   ============================================================================ */
CREATE PROCEDURE CRE_FORECAST_PR
    @BuyerId INT, @Month INT, @Year INT, @AmountMWh DECIMAL(18,4), @Status VARCHAR(15), @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblForecast (BuyerId, Month, Year, AmountMWh, Status, Created)
    VALUES (@BuyerId, @Month, @Year, @AmountMWh, @Status, @Created);
END
GO
CREATE PROCEDURE UPD_FORECAST_PR @Id INT, @AmountMWh DECIMAL(18,4), @Status VARCHAR(15), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblForecast SET AmountMWh=@AmountMWh, Status=@Status, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ID_FORECAST_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblForecast WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_FORECAST_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblForecast ORDER BY Id DESC;
END
GO
CREATE PROCEDURE RET_BY_BUYER_FORECAST_PR @BuyerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblForecast WHERE BuyerId=@BuyerId ORDER BY Year DESC, Month DESC;
END
GO
CREATE PROCEDURE RET_BY_MONTH_FORECAST_PR @Month INT, @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblForecast WHERE Month=@Month AND Year=@Year;
END
GO
CREATE PROCEDURE RET_ACTIVE_BY_BUYER_MONTH_FORECAST_PR @BuyerId INT, @Month INT, @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM tblForecast WHERE BuyerId=@BuyerId AND Month=@Month AND Year=@Year AND Status <> 'Cancelled';
END
GO
CREATE PROCEDURE UPD_STATUS_FORECAST_PR @Id INT, @Status VARCHAR(15), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblForecast SET Status=@Status, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE BLOCK_MONTH_FORECAST_PR @Month INT, @Year INT, @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblForecast SET Status='Blocked', Updated=@Updated
    WHERE Month=@Month AND Year=@Year AND Status IN ('Pending','Modified');
END
GO
CREATE PROCEDURE CANCEL_BEYOND_3M_FORECAST_PR @BuyerId INT, @StartMonth INT, @StartYear INT, @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblForecast SET Status='Cancelled', Updated=@Updated
    WHERE BuyerId=@BuyerId
      AND Status NOT IN ('Cancelled','Distributed')
      AND (Year > @StartYear OR (Year = @StartYear AND Month > @StartMonth));
END
GO


/* ============================================================================
   COMMERCIAL DISTRIBUTION
   ============================================================================ */
CREATE PROCEDURE CRE_COMM_DIST_PR
    @Month INT, @Year INT, @ExecutionDate DATETIME2, @AvailableInventory DECIMAL(18,4),
    @TotalDemand DECIMAL(18,4), @DistributedEnergy DECIMAL(18,4), @RoundingResidual DECIMAL(18,4),
    @Scenario VARCHAR(20), @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblCommercialDistribution (Month, Year, ExecutionDate, AvailableInventory, TotalDemand, DistributedEnergy, RoundingResidual, Scenario, Created)
    VALUES (@Month, @Year, @ExecutionDate, @AvailableInventory, @TotalDemand, @DistributedEnergy, @RoundingResidual, @Scenario, @Created);
END
GO
CREATE PROCEDURE RET_ID_COMM_DIST_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCommercialDistribution WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_COMM_DIST_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCommercialDistribution ORDER BY Year DESC, Month DESC;
END
GO
CREATE PROCEDURE RET_BY_MONTH_COMM_DIST_PR @Month INT, @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblCommercialDistribution WHERE Month=@Month AND Year=@Year;
END
GO


/* ============================================================================
   DISTRIBUTION DETAIL
   ============================================================================ */
CREATE PROCEDURE CRE_DIST_DTL_PR
    @DistributionId INT, @BuyerId INT, @ForecastId INT, @RequestedMWh DECIMAL(18,4),
    @AssignedMWh DECIMAL(18,4), @UnsuppliedDemand DECIMAL(18,4), @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblDistributionDetail (DistributionId, BuyerId, ForecastId, RequestedMWh, AssignedMWh, UnsuppliedDemand, Created)
    VALUES (@DistributionId, @BuyerId, @ForecastId, @RequestedMWh, @AssignedMWh, @UnsuppliedDemand, @Created);
END
GO
CREATE PROCEDURE RET_ID_DIST_DTL_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblDistributionDetail WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_BY_DIST_DIST_DTL_PR @DistributionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblDistributionDetail WHERE DistributionId=@DistributionId;
END
GO
CREATE PROCEDURE RET_BY_BUYER_DIST_DTL_PR @BuyerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblDistributionDetail WHERE BuyerId=@BuyerId ORDER BY Id DESC;
END
GO


/* ============================================================================
   PRICE  (CRE_PRICE_PR auto-closes the previously active price)
   ============================================================================ */
CREATE PROCEDURE CRE_PRICE_PR
    @PriceCRCPerMWh DECIMAL(18,4), @ValidFrom DATETIME2, @ValidTo DATETIME2 = NULL, @IsActive BIT, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblPrice SET IsActive = 0, ValidTo = @ValidFrom, Updated = @Created WHERE IsActive = 1;
    INSERT INTO tblPrice (PriceCRCPerMWh, ValidFrom, ValidTo, IsActive, Created)
    VALUES (@PriceCRCPerMWh, @ValidFrom, @ValidTo, @IsActive, @Created);
END
GO
CREATE PROCEDURE RET_ID_PRICE_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblPrice WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_PRICE_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblPrice ORDER BY ValidFrom DESC;
END
GO
CREATE PROCEDURE RET_ACTIVE_PRICE_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM tblPrice WHERE IsActive=1 ORDER BY ValidFrom DESC;
END
GO
CREATE PROCEDURE RET_AT_DATETIME_PRICE_PR @Timestamp DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM tblPrice WHERE ValidFrom <= @Timestamp AND (ValidTo IS NULL OR ValidTo > @Timestamp) ORDER BY ValidFrom DESC;
END
GO


/* ============================================================================
   TAX  (CRE_TAX_PR auto-closes the previously active tax)
   ============================================================================ */
CREATE PROCEDURE CRE_TAX_PR
    @Name VARCHAR(50), @Percentage DECIMAL(18,4), @ValidFrom DATETIME2, @ValidTo DATETIME2 = NULL, @IsActive BIT, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblTax SET IsActive = 0, ValidTo = @ValidFrom, Updated = @Created WHERE IsActive = 1;
    INSERT INTO tblTax (Name, Percentage, ValidFrom, ValidTo, IsActive, Created)
    VALUES (@Name, @Percentage, @ValidFrom, @ValidTo, @IsActive, @Created);
END
GO
CREATE PROCEDURE RET_ID_TAX_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblTax WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_TAX_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblTax ORDER BY ValidFrom DESC;
END
GO
CREATE PROCEDURE RET_ACTIVE_TAX_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM tblTax WHERE IsActive=1 ORDER BY ValidFrom DESC;
END
GO
CREATE PROCEDURE RET_AT_DATETIME_TAX_PR @Timestamp DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM tblTax WHERE ValidFrom <= @Timestamp AND (ValidTo IS NULL OR ValidTo > @Timestamp) ORDER BY ValidFrom DESC;
END
GO


/* ============================================================================
   ACCOUNT STATEMENT (WORM parcial)
   ============================================================================ */
CREATE PROCEDURE CRE_ACCT_STMT_PR
    @BuyerId INT, @DistributionId INT, @ForecastId INT, @Month INT, @Year INT,
    @AssignedMWh DECIMAL(18,4), @UnitPrice DECIMAL(18,4), @TaxPercentage DECIMAL(18,4),
    @Subtotal DECIMAL(18,4), @TaxAmount DECIMAL(18,4), @Total DECIMAL(18,4), @Status VARCHAR(10),
    @RevisionNumber INT, @ParentId INT = NULL, @AnnulmentReason VARCHAR(500) = NULL, @IssueDate DATETIME2, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblAccountStatement
        (BuyerId, DistributionId, ForecastId, Month, Year, AssignedMWh, UnitPrice, TaxPercentage, Subtotal, TaxAmount, Total, Status, RevisionNumber, ParentId, AnnulmentReason, IssueDate, Created)
    VALUES
        (@BuyerId, @DistributionId, @ForecastId, @Month, @Year, @AssignedMWh, @UnitPrice, @TaxPercentage, @Subtotal, @TaxAmount, @Total, @Status, @RevisionNumber, @ParentId, @AnnulmentReason, @IssueDate, @Created);
END
GO
CREATE PROCEDURE RET_ID_ACCT_STMT_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAccountStatement WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_ACCT_STMT_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAccountStatement ORDER BY Id DESC;
END
GO
CREATE PROCEDURE RET_BY_BUYER_ACCT_STMT_PR @BuyerId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAccountStatement WHERE BuyerId=@BuyerId ORDER BY Year DESC, Month DESC, RevisionNumber DESC;
END
GO
CREATE PROCEDURE RET_BY_DIST_ACCT_STMT_PR @DistributionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAccountStatement WHERE DistributionId=@DistributionId;
END
GO
CREATE PROCEDURE UPD_ANNUL_ACCT_STMT_PR @Id INT, @Status VARCHAR(10), @AnnulmentReason VARCHAR(500), @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblAccountStatement SET Status=@Status, AnnulmentReason=@AnnulmentReason, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_CURRENT_VERSION_ACCT_STMT_PR @BuyerId INT, @Month INT, @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 * FROM tblAccountStatement
    WHERE BuyerId=@BuyerId AND Month=@Month AND Year=@Year
    ORDER BY RevisionNumber DESC;
END
GO


/* ============================================================================
   NOTIFICATION QUEUE
   ============================================================================ */
CREATE PROCEDURE CRE_NOTIF_PR
    @UserId INT, @RecipientEmail VARCHAR(250), @NotificationType VARCHAR(40), @Subject VARCHAR(250),
    @Body NVARCHAR(MAX), @IsCritical BIT, @Status VARCHAR(10), @Attempts INT, @NextAttempt DATETIME2 = NULL,
    @SentDate DATETIME2 = NULL, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblNotificationQueue (UserId, RecipientEmail, NotificationType, Subject, Body, IsCritical, Status, Attempts, NextAttempt, SentDate, Created)
    VALUES (@UserId, @RecipientEmail, @NotificationType, @Subject, @Body, @IsCritical, @Status, @Attempts, @NextAttempt, @SentDate, @Created);
END
GO
CREATE PROCEDURE UPD_NOTIF_PR
    @Id INT, @Status VARCHAR(10), @Attempts INT, @NextAttempt DATETIME2 = NULL, @SentDate DATETIME2 = NULL, @Updated DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblNotificationQueue SET Status=@Status, Attempts=@Attempts, NextAttempt=@NextAttempt, SentDate=@SentDate, Updated=@Updated WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ID_NOTIF_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblNotificationQueue WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_PENDING_NOTIF_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblNotificationQueue WHERE Status='Pending' AND (NextAttempt IS NULL OR NextAttempt <= SYSUTCDATETIME()) ORDER BY IsCritical DESC, Id;
END
GO
CREATE PROCEDURE RET_BY_USER_NOTIF_PR @UserId INT, @PageNumber INT, @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblNotificationQueue WHERE UserId=@UserId ORDER BY Id DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO


/* ============================================================================
   AUDIT LOG (WORM)
   ============================================================================ */
CREATE PROCEDURE CRE_AUDIT_LOG_PR
    @UserId INT = NULL, @UserName VARCHAR(200), @Module VARCHAR(30), @Action VARCHAR(20),
    @AffectedEntity VARCHAR(100), @EntityId INT, @PreviousValue NVARCHAR(MAX) = NULL,
    @NewValue NVARCHAR(MAX) = NULL, @EventDate DATETIME2, @IsColdArchive BIT, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblAuditLog (UserId, UserName, Module, Action, AffectedEntity, EntityId, PreviousValue, NewValue, EventDate, IsColdArchive, Created)
    VALUES (@UserId, @UserName, @Module, @Action, @AffectedEntity, @EntityId, @PreviousValue, @NewValue, @EventDate, @IsColdArchive, @Created);
END
GO
CREATE PROCEDURE RET_ID_AUDIT_LOG_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAuditLog WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_BY_MODULE_AUDIT_LOG_PR @Module VARCHAR(30), @PageNumber INT, @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAuditLog WHERE Module=@Module ORDER BY EventDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
CREATE PROCEDURE RET_BY_USER_AUDIT_LOG_PR @UserId INT, @PageNumber INT, @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAuditLog WHERE UserId=@UserId ORDER BY EventDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
CREATE PROCEDURE RET_BY_DATE_AUDIT_LOG_PR @From DATETIME2, @To DATETIME2, @PageNumber INT, @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblAuditLog WHERE EventDate BETWEEN @From AND @To ORDER BY EventDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
CREATE PROCEDURE MARK_COLD_AUDIT_LOG_PR @Threshold DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE tblAuditLog SET IsColdArchive = 1 WHERE EventDate <= @Threshold AND IsColdArchive = 0;
END
GO


/* ============================================================================
   EXPORT LOG (WORM)
   ----------------------------------------------------------------------------
   NOTE: ExportLogCrudFactory.RetrieveAll<T>() and RetrieveAllPaged(page,size)
   both target "RET_ALL_EXPORT_LOG_PR" with a different parameter count.
   Resolved via optional parameters: without them it returns all rows; with them, it paginates.
   ============================================================================ */
CREATE PROCEDURE CRE_EXPORT_LOG_PR
    @UserId INT, @DocumentType VARCHAR(50), @DocumentId INT, @Format VARCHAR(5), @CloneFilePath VARCHAR(500), @EventDate DATETIME2, @Created DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO tblExportLog (UserId, DocumentType, DocumentId, Format, CloneFilePath, EventDate, Created)
    VALUES (@UserId, @DocumentType, @DocumentId, @Format, @CloneFilePath, @EventDate, @Created);
END
GO
CREATE PROCEDURE RET_ID_EXPORT_LOG_PR @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblExportLog WHERE Id=@Id;
END
GO
CREATE PROCEDURE RET_ALL_EXPORT_LOG_PR @PageNumber INT = NULL, @PageSize INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageNumber IS NULL
        SELECT * FROM tblExportLog ORDER BY Id DESC;
    ELSE
        SELECT * FROM tblExportLog ORDER BY Id DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
CREATE PROCEDURE RET_BY_USER_EXPORT_LOG_PR @UserId INT, @PageNumber INT, @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM tblExportLog WHERE UserId=@UserId ORDER BY Id DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO


/* ============================================================================
   END OF 02_Schema_StoredProcedures.sql
   ============================================================================ */
