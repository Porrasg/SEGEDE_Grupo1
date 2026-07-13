/* ============================================================================
   SGDE - Electric Generation and Dispatch System
   Database Schema | SQL Server / Azure SQL Database
   CoreForge Technologies | BISOFT-13
   ----------------------------------------------------------------------------
   Naming conventions:
     - Tables:   tbl{EntityName} (PascalCase, English)
     - Columns:  PascalCase, English, matching C# entity properties
     - Audit cols: Created / Updated  (match BaseDTO: Id, Created, Updated)
   Notes:
     - Energy in MWh, DECIMAL(18,4) everywhere for 4-decimal precision
     - All datetimes stored in America/Costa_Rica (handled in app layer)
     - WORM tables protected by INSTEAD OF triggers (see section 8)
     - Singletons (CentralBank, FlushConfig) locked to Id = 1
   ----------------------------------------------------------------------------
   Este archivo asume que ya estás conectado directamente a la base de datos
   destino (p.ej. ProyectoCenfoGp1 en Azure SQL) — no crea ni selecciona base
   de datos, a diferencia del script original de referencia.
   ============================================================================ */


/* ============================================================================
   SECTION 1 - CORE TABLES
   ============================================================================ */

-- ----------------------------------------------------------------------------
-- Users
-- ----------------------------------------------------------------------------
CREATE TABLE tblUsers
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    Identification      VARCHAR(12)   NOT NULL UNIQUE,
    FirstName           VARCHAR(150)  NOT NULL,
    LastName            VARCHAR(150)  NOT NULL,
    BirthDate           DATE          NOT NULL,
    Phone               VARCHAR(20)   NOT NULL,
    Email               VARCHAR(250)  NOT NULL UNIQUE,
    PhotoUrl            VARCHAR(MAX)  NULL,   -- URL o data-URL base64 de la foto de perfil
    PasswordHash        VARCHAR(512)  NOT NULL,
    Role                VARCHAR(30)   NOT NULL,   -- Administrator | Engineer | Buyer
    Status              VARCHAR(30)   NOT NULL,   -- PendingActivation | Active | Inactive | Blocked
    FailedAttempts      INT           NOT NULL DEFAULT 0,
    BlockedAt           DATETIME2     NULL,
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL
);
GO

-- ----------------------------------------------------------------------------
-- OTP Attempts (local control over the external OTP service)
-- ----------------------------------------------------------------------------
CREATE TABLE tblOtpAttempts
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    UserId              INT           NOT NULL,
    UsageType           VARCHAR(20)   NOT NULL,   -- Activation | Login | Recovery
    ResendCount         INT           NOT NULL DEFAULT 0,
    FailedAttempts      INT           NOT NULL DEFAULT 0,
    Status              VARCHAR(15)   NOT NULL,   -- InProgress | Verified | Blocked
    StartDate           DATETIME2     NOT NULL,
    WindowExpiration    DATETIME2     NOT NULL,
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT FK_OtpAttempts_Users
        FOREIGN KEY (UserId) REFERENCES tblUsers(Id) ON DELETE CASCADE
);
GO


/* ============================================================================
   SECTION 2 - OPERATIONS (Turbines, Maintenance, Failures, Energy)
   ============================================================================ */

-- ----------------------------------------------------------------------------
-- Turbines
-- ----------------------------------------------------------------------------
CREATE TABLE tblTurbines
(
    Id                      INT IDENTITY(1,1) PRIMARY KEY,
    UniqueCode              VARCHAR(50)   NOT NULL UNIQUE,
    Name                    VARCHAR(150)  NOT NULL,
    Location                VARCHAR(300)  NOT NULL,
    Brand                   VARCHAR(100)  NOT NULL,
    Model                   VARCHAR(100)  NOT NULL,
    Year                    INT           NOT NULL,
    WeeklyNominalCapacity   DECIMAL(18,4) NOT NULL,   -- MWh, > 0
    Status                  VARCHAR(40)   NOT NULL,   -- Active | UnderMaintenance | Damaged | SuspendedForNonCompliance | Decommissioned
    LastMaintenance         DATETIME2     NULL,
    LastStateChange         DATETIME2     NOT NULL,
    Created                 DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated                 DATETIME2     NULL,

    CONSTRAINT CK_Turbines_Capacity CHECK (WeeklyNominalCapacity > 0)
);
GO

-- ----------------------------------------------------------------------------
-- Turbine State History
-- ----------------------------------------------------------------------------
CREATE TABLE tblTurbineStateHistory
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TurbineId       INT           NOT NULL,
    PreviousState   VARCHAR(40)   NOT NULL,
    NewState        VARCHAR(40)   NOT NULL,
    ChangeDate      DATETIME2     NOT NULL,
    Reason          VARCHAR(500)  NOT NULL,
    UserId          INT           NOT NULL,   -- 0 = System actor (see note below)
    Created         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated         DATETIME2     NULL,

    CONSTRAINT FK_TurbineStateHistory_Turbines
        FOREIGN KEY (TurbineId) REFERENCES tblTurbines(Id) ON DELETE CASCADE
    -- NOTE: no FK on UserId because 0 (System actor) is a sentinel, not a real
    -- user row. Application enforces validity. A real FK would reject Id = 0.
);
GO

-- ----------------------------------------------------------------------------
-- Local Battery (1:1 with Turbine)
-- ----------------------------------------------------------------------------
CREATE TABLE tblLocalBattery
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TurbineId       INT           NOT NULL UNIQUE,
    StoredEnergy    DECIMAL(18,4) NOT NULL DEFAULT 0,   -- MWh, >= 0
    Created         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated         DATETIME2     NULL,

    CONSTRAINT FK_LocalBattery_Turbines
        FOREIGN KEY (TurbineId) REFERENCES tblTurbines(Id) ON DELETE CASCADE,
    CONSTRAINT CK_LocalBattery_NonNegative CHECK (StoredEnergy >= 0)
);
GO

-- ----------------------------------------------------------------------------
-- Maintenances
-- ----------------------------------------------------------------------------
CREATE TABLE tblMaintenances
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    TurbineId           INT           NOT NULL,
    MaintenanceType     VARCHAR(15)   NOT NULL,   -- Preventive | Corrective
    EstimatedStartDate  DATETIME2     NOT NULL,
    EstimatedEndDate    DATETIME2     NOT NULL,
    ActualStartDate     DATETIME2     NULL,
    ActualEndDate       DATETIME2     NULL,
    Result              VARCHAR(500)  NULL,
    Status              VARCHAR(15)   NOT NULL,   -- Scheduled | InProgress | Completed | Cancelled
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT FK_Maintenances_Turbines
        FOREIGN KEY (TurbineId) REFERENCES tblTurbines(Id) ON DELETE CASCADE
);
GO

-- ----------------------------------------------------------------------------
-- Failures
-- ----------------------------------------------------------------------------
CREATE TABLE tblFailures
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TurbineId       INT            NOT NULL,
    FailureDate     DATETIME2      NOT NULL,
    Description     VARCHAR(1000)  NOT NULL,
    Severity        VARCHAR(10)    NOT NULL,   -- Normal | Critical
    Created         DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated         DATETIME2      NULL,

    CONSTRAINT FK_Failures_Turbines
        FOREIGN KEY (TurbineId) REFERENCES tblTurbines(Id) ON DELETE CASCADE
);
GO

-- ----------------------------------------------------------------------------
-- Energy Generation Log (WORM; Updated present because BaseDTO/CrudFactory
-- always read/write it, even though the app never actually updates a row)
-- ----------------------------------------------------------------------------
CREATE TABLE tblEnergyGenerationLog
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    TurbineId           INT           NOT NULL,
    ActiveTimeSeconds   DECIMAL(18,4) NOT NULL,
    GeneratedEnergy     DECIMAL(18,4) NOT NULL,   -- MWh
    EventDate           DATETIME2     NOT NULL,
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT FK_EnergyGenerationLog_Turbines
        FOREIGN KEY (TurbineId) REFERENCES tblTurbines(Id) ON DELETE CASCADE
);
GO

-- ----------------------------------------------------------------------------
-- Energy Loss Log (WORM; Updated present because BaseDTO/CrudFactory always
-- read/write it, even though the app never actually updates a row)
-- ----------------------------------------------------------------------------
CREATE TABLE tblEnergyLossLog
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    TurbineId           INT           NOT NULL,
    InactiveTimeSeconds DECIMAL(18,4) NOT NULL,
    LostEnergy          DECIMAL(18,4) NOT NULL,   -- MWh
    Cause               VARCHAR(25)   NOT NULL,   -- Maintenance | Failure | Suspension | Decommission | Transition | Other
    EventDate           DATETIME2     NOT NULL,
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT FK_EnergyLossLog_Turbines
        FOREIGN KEY (TurbineId) REFERENCES tblTurbines(Id) ON DELETE CASCADE
);
GO


/* ============================================================================
   SECTION 3 - FLUSH, CENTRAL BANK
   ============================================================================ */

-- ----------------------------------------------------------------------------
-- Flush Config (SINGLETON - Id locked to 1, no IDENTITY)
-- ----------------------------------------------------------------------------
CREATE TABLE tblFlushConfig
(
    Id              INT           NOT NULL PRIMARY KEY,
    ExecutionTime   TIME          NOT NULL DEFAULT '00:00:00',
    IsAutomatic     BIT           NOT NULL DEFAULT 1,
    Created         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated         DATETIME2     NULL,

    CONSTRAINT CK_FlushConfig_Singleton CHECK (Id = 1)
);
GO

-- ----------------------------------------------------------------------------
-- Central Bank (SINGLETON - Id locked to 1, no IDENTITY)
-- ----------------------------------------------------------------------------
CREATE TABLE tblCentralBank
(
    Id                  INT           NOT NULL PRIMARY KEY,
    CurrentInventory    DECIMAL(18,4) NOT NULL DEFAULT 0,   -- MWh
    ManualCapacity      DECIMAL(18,4) NULL,                 -- takes priority if set
    AutomaticCapacity   DECIMAL(18,4) NOT NULL DEFAULT 0,   -- sum of active turbines' capacity
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT CK_CentralBank_Singleton CHECK (Id = 1)
);
GO

-- ----------------------------------------------------------------------------
-- Flush (one row per flush execution)
-- ----------------------------------------------------------------------------
CREATE TABLE tblFlush
(
    Id                      INT IDENTITY(1,1) PRIMARY KEY,
    ExecutionType           VARCHAR(12)   NOT NULL,   -- Automatic | Manual
    Status                  VARCHAR(15)   NOT NULL,   -- Processing | Completed | Cancelled | Failed
    UserId                  INT           NULL,       -- null if automatic
    TotalTransferredEnergy  DECIMAL(18,4) NOT NULL DEFAULT 0,   -- MWh
    SaturationLoss          DECIMAL(18,4) NOT NULL DEFAULT 0,   -- MWh
    StartDate               DATETIME2     NOT NULL,
    EndDate                 DATETIME2     NULL,
    Created                 DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated                 DATETIME2     NULL,

    CONSTRAINT FK_Flush_Users
        FOREIGN KEY (UserId) REFERENCES tblUsers(Id)
);
GO

-- ----------------------------------------------------------------------------
-- Flush Snapshot (WORM - immutable, no Updated)
-- ----------------------------------------------------------------------------
CREATE TABLE tblFlushSnapshot
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    FlushId         INT           NOT NULL,
    TurbineId       INT           NOT NULL,
    LocalBatteryId  INT           NOT NULL,
    CapturedEnergy  DECIMAL(18,4) NOT NULL,   -- MWh at capture moment
    EventDate       DATETIME2     NOT NULL,
    Created         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated         DATETIME2     NULL,

    CONSTRAINT FK_FlushSnapshot_Flush
        FOREIGN KEY (FlushId) REFERENCES tblFlush(Id),
    CONSTRAINT FK_FlushSnapshot_Turbines
        FOREIGN KEY (TurbineId) REFERENCES tblTurbines(Id),
    CONSTRAINT FK_FlushSnapshot_LocalBattery
        FOREIGN KEY (LocalBatteryId) REFERENCES tblLocalBattery(Id)
);
GO

-- ----------------------------------------------------------------------------
-- Saturation Log (WORM - immutable, no Updated)
-- ----------------------------------------------------------------------------
CREATE TABLE tblSaturationLog
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    FlushId             INT           NOT NULL,
    PreviousInventory   DECIMAL(18,4) NOT NULL,   -- MWh before flush
    NewInventory        DECIMAL(18,4) NOT NULL,   -- MWh after (= max capacity)
    ExcessEnergy        DECIMAL(18,4) NOT NULL,   -- MWh discarded
    EventDate           DATETIME2     NOT NULL,
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT FK_SaturationLog_Flush
        FOREIGN KEY (FlushId) REFERENCES tblFlush(Id)
);
GO

-- ----------------------------------------------------------------------------
-- Central Bank Log (movement ledger)
-- ----------------------------------------------------------------------------
CREATE TABLE tblCentralBankLog
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    MovementType        VARCHAR(15)   NOT NULL,   -- Inflow | Outflow | Saturation
    Amount              DECIMAL(18,4) NOT NULL,   -- MWh
    ResultingInventory  DECIMAL(18,4) NOT NULL,   -- MWh after movement
    FlushId             INT           NULL,
    DistributionId      INT           NULL,
    EventDate           DATETIME2     NOT NULL,
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT FK_CentralBankLog_Flush
        FOREIGN KEY (FlushId) REFERENCES tblFlush(Id)
    -- FK to distribution added after tblCommercialDistribution is created (below)
);
GO


/* ============================================================================
   SECTION 4 - COMMERCIAL (Forecast, Distribution)
   ============================================================================ */

-- ----------------------------------------------------------------------------
-- Forecast
-- ----------------------------------------------------------------------------
CREATE TABLE tblForecast
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    BuyerId         INT           NOT NULL,
    Month           INT           NOT NULL,   -- 1-12 (INT, not TINYINT: ADO.NET maps TINYINT -> System.Byte, which the C# CrudFactories cannot direct-cast to int)
    Year            INT           NOT NULL,
    AmountMWh       DECIMAL(18,4) NOT NULL,   -- > 0
    Status          VARCHAR(15)   NOT NULL,   -- Pending | Modified | Blocked | Distributed | Cancelled
    Created         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated         DATETIME2     NULL,

    CONSTRAINT FK_Forecast_Users
        FOREIGN KEY (BuyerId) REFERENCES tblUsers(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Forecast_Buyer_Month_Year UNIQUE (BuyerId, Month, Year),
    CONSTRAINT CK_Forecast_Amount CHECK (AmountMWh > 0),
    CONSTRAINT CK_Forecast_Month CHECK (Month BETWEEN 1 AND 12)
);
GO

-- ----------------------------------------------------------------------------
-- Commercial Distribution (one per month)
-- ----------------------------------------------------------------------------
CREATE TABLE tblCommercialDistribution
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    Month               INT           NOT NULL,
    Year                INT           NOT NULL,
    ExecutionDate       DATETIME2     NOT NULL,
    AvailableInventory  DECIMAL(18,4) NOT NULL,
    TotalDemand         DECIMAL(18,4) NOT NULL,
    DistributedEnergy   DECIMAL(18,4) NOT NULL,
    RoundingResidual    DECIMAL(18,4) NOT NULL,
    Scenario            VARCHAR(20)   NOT NULL,   -- ZeroDemand | ZeroInventory | Sufficient | Shortage
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT UQ_Distribution_Month_Year UNIQUE (Month, Year),
    CONSTRAINT CK_Distribution_Month CHECK (Month BETWEEN 1 AND 12)
);
GO

-- Deferred FK: CentralBankLog -> CommercialDistribution
ALTER TABLE tblCentralBankLog
    ADD CONSTRAINT FK_CentralBankLog_Distribution
        FOREIGN KEY (DistributionId) REFERENCES tblCommercialDistribution(Id);
GO

-- ----------------------------------------------------------------------------
-- Distribution Detail (one per buyer per distribution)
-- ----------------------------------------------------------------------------
CREATE TABLE tblDistributionDetail
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    DistributionId      INT           NOT NULL,
    BuyerId             INT           NOT NULL,
    ForecastId          INT           NOT NULL,
    RequestedMWh        DECIMAL(18,4) NOT NULL,
    AssignedMWh         DECIMAL(18,4) NOT NULL,
    UnsuppliedDemand    DECIMAL(18,4) NOT NULL,
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT FK_DistributionDetail_Distribution
        FOREIGN KEY (DistributionId) REFERENCES tblCommercialDistribution(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DistributionDetail_Users
        FOREIGN KEY (BuyerId) REFERENCES tblUsers(Id),
    CONSTRAINT FK_DistributionDetail_Forecast
        FOREIGN KEY (ForecastId) REFERENCES tblForecast(Id)
);
GO


/* ============================================================================
   SECTION 5 - BILLING (Price, Tax, AccountStatement)
   ============================================================================ */

-- ----------------------------------------------------------------------------
-- Price (history; one active at a time)
-- ----------------------------------------------------------------------------
CREATE TABLE tblPrice
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    PriceCRCPerMWh      DECIMAL(18,4) NOT NULL,   -- > 0
    ValidFrom           DATETIME2     NOT NULL,
    ValidTo             DATETIME2     NULL,        -- null = currently active
    IsActive            BIT           NOT NULL DEFAULT 1,
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT CK_Price_Positive CHECK (PriceCRCPerMWh > 0)
);
GO

-- ----------------------------------------------------------------------------
-- Tax (history; one active at a time)
-- ----------------------------------------------------------------------------
CREATE TABLE tblTax
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            VARCHAR(50)   NOT NULL,        -- e.g. VAT
    Percentage      DECIMAL(18,4) NOT NULL,        -- fraction: 0.1300 = 13%
    ValidFrom       DATETIME2     NOT NULL,
    ValidTo         DATETIME2     NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    Created         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated         DATETIME2     NULL,

    CONSTRAINT CK_Tax_Range CHECK (Percentage >= 0 AND Percentage < 1)
);
GO

-- ----------------------------------------------------------------------------
-- Account Statement (WORM on financial fields; Status/AnnulmentReason mutable)
-- NOTE: NO cascade on any FK - statements are legally immutable (RF-066).
--       'RevisionNumber' used instead of reserved word 'Version'.
-- ----------------------------------------------------------------------------
CREATE TABLE tblAccountStatement
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    BuyerId             INT           NOT NULL,
    DistributionId      INT           NOT NULL,
    ForecastId          INT           NOT NULL,
    Month               INT           NOT NULL,
    Year                INT           NOT NULL,
    AssignedMWh         DECIMAL(18,4) NOT NULL,   -- frozen
    UnitPrice           DECIMAL(18,4) NOT NULL,   -- frozen
    TaxPercentage       DECIMAL(18,4) NOT NULL,   -- frozen
    Subtotal            DECIMAL(18,4) NOT NULL,   -- frozen
    TaxAmount           DECIMAL(18,4) NOT NULL,   -- frozen
    Total               DECIMAL(18,4) NOT NULL,   -- frozen
    Status              VARCHAR(10)   NOT NULL,   -- Issued | Annulled  (mutable)
    RevisionNumber      INT           NOT NULL DEFAULT 0,
    ParentId            INT           NULL,
    AnnulmentReason     VARCHAR(500)  NULL,       -- mutable
    IssueDate           DATETIME2     NOT NULL,
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT FK_AccountStatement_Users
        FOREIGN KEY (BuyerId) REFERENCES tblUsers(Id),
    CONSTRAINT FK_AccountStatement_Distribution
        FOREIGN KEY (DistributionId) REFERENCES tblCommercialDistribution(Id),
    CONSTRAINT FK_AccountStatement_Forecast
        FOREIGN KEY (ForecastId) REFERENCES tblForecast(Id),
    CONSTRAINT FK_AccountStatement_Parent
        FOREIGN KEY (ParentId) REFERENCES tblAccountStatement(Id)
);
GO


/* ============================================================================
   SECTION 6 - NOTIFICATIONS, AUDIT, EXPORT
   ============================================================================ */

-- ----------------------------------------------------------------------------
-- Notification Queue
-- ----------------------------------------------------------------------------
CREATE TABLE tblNotificationQueue
(
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    UserId              INT           NOT NULL,
    RecipientEmail      VARCHAR(250)  NOT NULL,
    NotificationType    VARCHAR(40)   NOT NULL,
    Subject             VARCHAR(250)  NOT NULL,
    Body                NVARCHAR(MAX) NOT NULL,
    IsCritical          BIT           NOT NULL DEFAULT 0,
    Status              VARCHAR(10)   NOT NULL,   -- Pending | Sent | Failed
    Attempts            INT           NOT NULL DEFAULT 0,
    NextAttempt         DATETIME2     NULL,
    SentDate            DATETIME2     NULL,
    Created             DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated             DATETIME2     NULL,

    CONSTRAINT FK_NotificationQueue_Users
        FOREIGN KEY (UserId) REFERENCES tblUsers(Id) ON DELETE CASCADE
);
GO

-- ----------------------------------------------------------------------------
-- Audit Log (WORM - immutable)
-- ----------------------------------------------------------------------------
CREATE TABLE tblAuditLog
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT           NULL,        -- null = System actor
    UserName        VARCHAR(200)  NOT NULL,
    Module          VARCHAR(30)   NOT NULL,
    Action          VARCHAR(20)   NOT NULL,
    AffectedEntity  VARCHAR(100)  NOT NULL,
    EntityId        INT           NOT NULL,
    PreviousValue   NVARCHAR(MAX) NULL,
    NewValue        NVARCHAR(MAX) NULL,
    EventDate       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsColdArchive   BIT           NOT NULL DEFAULT 0,
    Created         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated         DATETIME2     NULL,

    CONSTRAINT FK_AuditLog_Users
        FOREIGN KEY (UserId) REFERENCES tblUsers(Id)
);
GO

-- ----------------------------------------------------------------------------
-- Export Log (WORM - immutable)
-- ----------------------------------------------------------------------------
CREATE TABLE tblExportLog
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT           NOT NULL,
    DocumentType    VARCHAR(50)   NOT NULL,
    DocumentId      INT           NOT NULL,
    Format          VARCHAR(5)    NOT NULL,   -- CSV | EXCEL | HTML
    CloneFilePath   VARCHAR(500)  NOT NULL,
    EventDate       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Created         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Updated         DATETIME2     NULL,

    CONSTRAINT FK_ExportLog_Users
        FOREIGN KEY (UserId) REFERENCES tblUsers(Id)
);
GO


/* ============================================================================
   SECTION 7 - INDEXES (performance for common lookups)
   ============================================================================ */

CREATE INDEX IX_OtpAttempts_User_Type   ON tblOtpAttempts(UserId, UsageType);
CREATE INDEX IX_TurbineStateHistory_Trb ON tblTurbineStateHistory(TurbineId);
CREATE INDEX IX_Maintenances_Turbine    ON tblMaintenances(TurbineId);
CREATE INDEX IX_Failures_Turbine        ON tblFailures(TurbineId);
CREATE INDEX IX_EnergyGen_Turbine       ON tblEnergyGenerationLog(TurbineId);
CREATE INDEX IX_EnergyLoss_Turbine      ON tblEnergyLossLog(TurbineId);
CREATE INDEX IX_FlushSnapshot_Flush     ON tblFlushSnapshot(FlushId);
CREATE INDEX IX_SaturationLog_Flush     ON tblSaturationLog(FlushId);
CREATE INDEX IX_CentralBankLog_Flush    ON tblCentralBankLog(FlushId);
CREATE INDEX IX_CentralBankLog_Dist     ON tblCentralBankLog(DistributionId);
CREATE INDEX IX_Forecast_Buyer          ON tblForecast(BuyerId);
CREATE INDEX IX_Forecast_Month_Year     ON tblForecast(Month, Year);
CREATE INDEX IX_DistDetail_Distribution ON tblDistributionDetail(DistributionId);
CREATE INDEX IX_DistDetail_Buyer        ON tblDistributionDetail(BuyerId);
CREATE INDEX IX_AccountStatement_Buyer  ON tblAccountStatement(BuyerId);
CREATE INDEX IX_AccountStatement_Dist   ON tblAccountStatement(DistributionId);
CREATE INDEX IX_AuditLog_Module         ON tblAuditLog(Module);
CREATE INDEX IX_AuditLog_User           ON tblAuditLog(UserId);
CREATE INDEX IX_AuditLog_EventDate      ON tblAuditLog(EventDate);
CREATE INDEX IX_NotificationQueue_Stat  ON tblNotificationQueue(Status, NextAttempt);
CREATE INDEX IX_ExportLog_User          ON tblExportLog(UserId);
GO


/* ============================================================================
   SECTION 8 - WORM PROTECTION (INSTEAD OF triggers block UPDATE/DELETE)
   ----------------------------------------------------------------------------
   These tables are write-once. Triggers reject any modification attempt at the
   database level, independent of application logic.
   AccountStatement is partially WORM: handled separately (see below) because
   Status and AnnulmentReason must remain updatable via the controlled SP.
   ============================================================================ */

-- tblFlushSnapshot: fully immutable
CREATE TRIGGER TR_FlushSnapshot_NoUpdate ON tblFlushSnapshot
INSTEAD OF UPDATE AS
BEGIN
    THROW 50001, 'tblFlushSnapshot is WORM: updates are not allowed.', 1;
END;
GO
CREATE TRIGGER TR_FlushSnapshot_NoDelete ON tblFlushSnapshot
INSTEAD OF DELETE AS
BEGIN
    THROW 50001, 'tblFlushSnapshot is WORM: deletes are not allowed.', 1;
END;
GO

-- tblSaturationLog: fully immutable
CREATE TRIGGER TR_SaturationLog_NoUpdate ON tblSaturationLog
INSTEAD OF UPDATE AS
BEGIN
    THROW 50002, 'tblSaturationLog is WORM: updates are not allowed.', 1;
END;
GO
CREATE TRIGGER TR_SaturationLog_NoDelete ON tblSaturationLog
INSTEAD OF DELETE AS
BEGIN
    THROW 50002, 'tblSaturationLog is WORM: deletes are not allowed.', 1;
END;
GO

-- tblAuditLog: fully immutable (THE most critical WORM table)
CREATE TRIGGER TR_AuditLog_NoUpdate ON tblAuditLog
INSTEAD OF UPDATE AS
BEGIN
    -- Only allow the cold-archive flag to flip false->true; nothing else.
    IF UPDATE(IsColdArchive)
       AND NOT EXISTS (
           SELECT 1 FROM inserted i
           JOIN deleted d ON i.Id = d.Id
           WHERE i.UserId        <> d.UserId
              OR i.UserName       <> d.UserName
              OR i.Module         <> d.Module
              OR i.Action         <> d.Action
              OR i.AffectedEntity <> d.AffectedEntity
              OR i.EntityId       <> d.EntityId
              OR ISNULL(i.PreviousValue,'') <> ISNULL(d.PreviousValue,'')
              OR ISNULL(i.NewValue,'')      <> ISNULL(d.NewValue,'')
              OR i.EventDate      <> d.EventDate
       )
    BEGIN
        UPDATE a SET a.IsColdArchive = i.IsColdArchive
        FROM tblAuditLog a
        JOIN inserted i ON a.Id = i.Id;
    END
    ELSE
        THROW 50003, 'tblAuditLog is WORM: only the cold-archive flag may change.', 1;
END;
GO
CREATE TRIGGER TR_AuditLog_NoDelete ON tblAuditLog
INSTEAD OF DELETE AS
BEGIN
    THROW 50003, 'tblAuditLog is WORM: deletes are not allowed.', 1;
END;
GO

-- tblExportLog: fully immutable
CREATE TRIGGER TR_ExportLog_NoUpdate ON tblExportLog
INSTEAD OF UPDATE AS
BEGIN
    THROW 50004, 'tblExportLog is WORM: updates are not allowed.', 1;
END;
GO
CREATE TRIGGER TR_ExportLog_NoDelete ON tblExportLog
INSTEAD OF DELETE AS
BEGIN
    THROW 50004, 'tblExportLog is WORM: deletes are not allowed.', 1;
END;
GO

-- tblAccountStatement: financial fields immutable; only Status, AnnulmentReason,
-- Updated may change. DELETE never allowed (RF-066).
CREATE TRIGGER TR_AccountStatement_GuardUpdate ON tblAccountStatement
INSTEAD OF UPDATE AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN deleted d ON i.Id = d.Id
        WHERE i.BuyerId        <> d.BuyerId
           OR i.DistributionId <> d.DistributionId
           OR i.ForecastId     <> d.ForecastId
           OR i.Month          <> d.Month
           OR i.Year           <> d.Year
           OR i.AssignedMWh    <> d.AssignedMWh
           OR i.UnitPrice      <> d.UnitPrice
           OR i.TaxPercentage  <> d.TaxPercentage
           OR i.Subtotal       <> d.Subtotal
           OR i.TaxAmount      <> d.TaxAmount
           OR i.Total          <> d.Total
           OR i.RevisionNumber <> d.RevisionNumber
           OR ISNULL(i.ParentId,-1) <> ISNULL(d.ParentId,-1)
           OR i.IssueDate      <> d.IssueDate
    )
        THROW 50005, 'tblAccountStatement: financial fields are immutable (WORM).', 1;

    UPDATE a
    SET a.Status          = i.Status,
        a.AnnulmentReason = i.AnnulmentReason,
        a.Updated         = i.Updated
    FROM tblAccountStatement a
    JOIN inserted i ON a.Id = i.Id;
END;
GO
CREATE TRIGGER TR_AccountStatement_NoDelete ON tblAccountStatement
INSTEAD OF DELETE AS
BEGIN
    THROW 50005, 'tblAccountStatement is WORM: deletes are not allowed (RF-066).', 1;
END;
GO


/* ============================================================================
   SECTION 9 - SEED DATA
   ----------------------------------------------------------------------------
   Essential initial data so the system is functional from the first deploy.
   - Singletons: FlushConfig and CentralBank (locked to Id = 1)
   - Initial Administrator user (password hash MUST be replaced before prod)
   - Initial Price and Tax so the first monthly distribution doesn't fail
   ============================================================================ */

-- Singleton: Flush Config (automatic flush at midnight)
INSERT INTO tblFlushConfig (Id, ExecutionTime, IsAutomatic)
VALUES (1, '00:00:00', 1);
GO

-- Singleton: Central Bank (empty inventory, zero capacity)
INSERT INTO tblCentralBank (Id, CurrentInventory, ManualCapacity, AutomaticCapacity)
VALUES (1, 0.0000, NULL, 0.0000);
GO

-- Initial Administrator user.
-- IMPORTANT: replace the placeholder hash with a real BCrypt hash generated by
-- the application before going live. Password must be changed on first login.
INSERT INTO tblUsers
(Identification, FirstName, LastName, BirthDate, Phone, Email,
 PasswordHash, Role, Status, FailedAttempts, Created)
VALUES
('100000000', 'Admin', 'System', '1990-01-01', '00000000', 'admin@sgde.cr',
 '$2a$12$REPLACE_WITH_REAL_BCRYPT_HASH_BEFORE_DEPLOY',
 'Administrator', 'Active', 0, SYSUTCDATETIME());
GO

-- Initial Price: 50,000 CRC per MWh (active, no end date)
INSERT INTO tblPrice (PriceCRCPerMWh, ValidFrom, ValidTo, IsActive)
VALUES (50000.0000, SYSUTCDATETIME(), NULL, 1);
GO

-- Initial Tax: VAT 13% (active, no end date)
INSERT INTO tblTax (Name, Percentage, ValidFrom, ValidTo, IsActive)
VALUES ('VAT', 0.1300, SYSUTCDATETIME(), NULL, 1);
GO


/* ============================================================================
   END OF 01_Schema_Tables.sql
   24 tables | WORM triggers | singletons | indexes | seeds
   Continúa en 02_Schema_StoredProcedures.sql
   ============================================================================ */
