CREATE DATABASE SEGEDE_GRPP1;
GO

USE SEGEDE_GRPP1;
GO

CREATE TABLE tblUsers
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    Identification VARCHAR(12) NOT NULL UNIQUE,

    FirstName VARCHAR(150) NOT NULL,

    LastName VARCHAR(150) NOT NULL,

    BirthDate DATETIME2 NOT NULL,

    Phone VARCHAR(20) NOT NULL,

    Email VARCHAR(250) NOT NULL UNIQUE,

    PhotoUrl VARCHAR(500) NULL,

    PasswordHash VARCHAR(512) NOT NULL,

    Role VARCHAR(30) NOT NULL,

    Status VARCHAR(30) NOT NULL,

    FailedAttempts INT NOT NULL DEFAULT(0),

    BlockedAt DATETIME2 NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL
);
GO

CREATE TABLE tblOtpAttempts
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    UserId INT NOT NULL,

    UsageType VARCHAR(20) NOT NULL,

    ResendCount INT NOT NULL DEFAULT(0),

    FailedAttempts INT NOT NULL DEFAULT(0),

    Status VARCHAR(20) NOT NULL,

    StartDate DATETIME2 NOT NULL,

    WindowExpiration DATETIME2 NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_OtpAttempt_User
        FOREIGN KEY(UserId)
        REFERENCES tblUsers(Id)
);
GO

CREATE TABLE tblTurbines
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    UniqueCode VARCHAR(50) NOT NULL UNIQUE,

    Name VARCHAR(150) NOT NULL,

    Location VARCHAR(300) NOT NULL,

    Brand VARCHAR(100) NOT NULL,

    Model VARCHAR(100) NOT NULL,

    Year INT NOT NULL,

    WeeklyNominalCapacity DECIMAL(18,4) NOT NULL,

    Status VARCHAR(40) NOT NULL,

    LastMaintenance DATETIME2 NULL,

    LastStateChange DATETIME2 NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL
);
GO

CREATE TABLE tblTurbineStateHistory
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    TurbineId INT NOT NULL,

    PreviousState VARCHAR(40) NOT NULL,

    NewState VARCHAR(40) NOT NULL,

    ChangeDate DATETIME2 NOT NULL,

    Reason VARCHAR(500) NOT NULL,

    UserId INT NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_TurbineStateHistory_Turbine
        FOREIGN KEY(TurbineId)
        REFERENCES tblTurbines(Id),

    CONSTRAINT FK_TurbineStateHistory_User
        FOREIGN KEY(UserId)
        REFERENCES tblUsers(Id)
);
GO

CREATE TABLE tblLocalBattery
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    TurbineId INT NOT NULL UNIQUE,

    StoredEnergy DECIMAL(18,4) NOT NULL DEFAULT(0),

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_LocalBattery_Turbine
        FOREIGN KEY(TurbineId)
        REFERENCES tblTurbines(Id)
);
GO

CREATE TABLE tblMaintenances
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    TurbineId INT NOT NULL,

    MaintenanceType VARCHAR(30) NOT NULL,

    EstimatedStartDate DATETIME2 NOT NULL,

    EstimatedEndDate DATETIME2 NOT NULL,

    ActualStartDate DATETIME2 NULL,

    ActualEndDate DATETIME2 NULL,

    Result VARCHAR(500) NULL,

    Status VARCHAR(20) NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_Maintenance_Turbine
        FOREIGN KEY(TurbineId)
        REFERENCES tblTurbines(Id)
);
GO

CREATE TABLE tblFailures
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    TurbineId INT NOT NULL,

    FailureDate DATETIME2 NOT NULL,

    Description VARCHAR(1000) NOT NULL,

    Severity VARCHAR(20) NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_Failure_Turbine
        FOREIGN KEY(TurbineId)
        REFERENCES tblTurbines(Id)
);
GO

CREATE TABLE tblCommercialDistribution
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    Month INT NOT NULL,

    Year INT NOT NULL,

    ExecutionDate DATETIME2 NOT NULL,

    AvailableInventory DECIMAL(18,4) NOT NULL,

    TotalDemand DECIMAL(18,4) NOT NULL,

    DistributedEnergy DECIMAL(18,4) NOT NULL,

    RoundingResidual DECIMAL(18,4) NOT NULL,

    Scenario VARCHAR(50) NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT UQ_CommercialDistribution
        UNIQUE (Month, Year)
);
GO

CREATE TABLE tblEnergyGenerationLog
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    TurbineId INT NOT NULL,

    ActiveTimeSeconds DECIMAL(18,4) NOT NULL,

    GeneratedEnergy DECIMAL(18,4) NOT NULL,

    EventDate DATETIME2 NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_EnergyGenerationLog_Turbine
        FOREIGN KEY (TurbineId)
        REFERENCES tblTurbines(Id)
);
GO

CREATE TABLE tblEnergyLossLog
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    TurbineId INT NOT NULL,

    InactiveTimeSeconds DECIMAL(18,4) NOT NULL,

    LostEnergy DECIMAL(18,4) NOT NULL,

    Cause VARCHAR(50) NOT NULL,

    EventDate DATETIME2 NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_EnergyLossLog_Turbine
        FOREIGN KEY (TurbineId)
        REFERENCES tblTurbines(Id)
);
GO

CREATE TABLE tblFlushConfig
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    ExecutionTime TIME NOT NULL,

    IsAutomatic BIT NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL
);
GO

CREATE TABLE tblFlush
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    ExecutionType VARCHAR(50) NOT NULL,

    Status VARCHAR(50) NOT NULL,

    UserId INT NULL,

    TotalTransferredEnergy DECIMAL(18,4) NOT NULL,

    SaturationLoss DECIMAL(18,4) NOT NULL,

    StartDate DATETIME2 NOT NULL,

    EndDate DATETIME2 NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_Flush_User
        FOREIGN KEY (UserId)
        REFERENCES tblUsers(Id)
);
GO

CREATE TABLE tblCentralBank
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CurrentInventory DECIMAL(18,4) NOT NULL,

    ManualCapacity DECIMAL(18,4) NULL,

    AutomaticCapacity DECIMAL(18,4) NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL
);
GO

CREATE TABLE tblFlushSnapshot
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FlushId INT NOT NULL,

    TurbineId INT NOT NULL,

    LocalBatteryId INT NOT NULL,

    CapturedEnergy DECIMAL(18,4) NOT NULL,

    EventDate DATETIME2 NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_FlushSnapshot_Flush
        FOREIGN KEY (FlushId)
        REFERENCES tblFlush(Id),

    CONSTRAINT FK_FlushSnapshot_Turbine
        FOREIGN KEY (TurbineId)
        REFERENCES tblTurbines(Id),

    CONSTRAINT FK_FlushSnapshot_LocalBattery
        FOREIGN KEY (LocalBatteryId)
        REFERENCES tblLocalBattery(Id)
);
GO

CREATE TABLE tblSaturationLog
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FlushId INT NOT NULL,

    PreviousInventory DECIMAL(18,4) NOT NULL,

    NewInventory DECIMAL(18,4) NOT NULL,

    ExcessEnergy DECIMAL(18,4) NOT NULL,

    EventDate DATETIME2 NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_SaturationLog_Flush
        FOREIGN KEY (FlushId)
        REFERENCES tblFlush(Id)
);
GO

CREATE TABLE tblCentralBankLog
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    MovementType VARCHAR(50) NOT NULL,

    Amount DECIMAL(18,4) NOT NULL,

    ResultingInventory DECIMAL(18,4) NOT NULL,

    FlushId INT NULL,

    DistributionId INT NULL,

    EventDate DATETIME2 NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_CentralBankLog_Flush
        FOREIGN KEY (FlushId)
        REFERENCES tblFlush(Id),

    CONSTRAINT FK_CentralBankLog_Distribution
        FOREIGN KEY (DistributionId)
        REFERENCES tblCommercialDistribution(Id)
);
GO

CREATE TABLE tblForecast
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    BuyerId INT NOT NULL,

    Month INT NOT NULL,

    Year INT NOT NULL,

    AmountMWh DECIMAL(18,4) NOT NULL,

    Status VARCHAR(50) NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT UQ_Forecast UNIQUE(BuyerId, Month, Year),

    CONSTRAINT FK_Forecast_User
        FOREIGN KEY(BuyerId)
        REFERENCES tblUsers(Id)
);
GO

CREATE TABLE tblDistributionDetail
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    DistributionId INT NOT NULL,

    BuyerId INT NOT NULL,

    ForecastId INT NOT NULL,

    RequestedMWh DECIMAL(18,4) NOT NULL,

    AssignedMWh DECIMAL(18,4) NOT NULL,

    UnsuppliedDemand DECIMAL(18,4) NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_DistributionDetail_Distribution
        FOREIGN KEY(DistributionId)
        REFERENCES tblCommercialDistribution(Id),

    CONSTRAINT FK_DistributionDetail_User
        FOREIGN KEY(BuyerId)
        REFERENCES tblUsers(Id),

    CONSTRAINT FK_DistributionDetail_Forecast
        FOREIGN KEY(ForecastId)
        REFERENCES tblForecast(Id)
);
GO

CREATE TABLE tblPrice
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    PriceCRCPerMWh DECIMAL(18,4) NOT NULL,

    ValidFrom DATETIME2 NOT NULL,

    ValidTo DATETIME2 NULL,

    IsActive BIT NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL
);
GO

CREATE TABLE tblTax
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    Name VARCHAR(100) NOT NULL,

    Percentage DECIMAL(18,4) NOT NULL,

    ValidFrom DATETIME2 NOT NULL,

    ValidTo DATETIME2 NULL,

    IsActive BIT NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL
);
GO

CREATE TABLE tblAccountStatement
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    BuyerId INT NOT NULL,

    DistributionId INT NOT NULL,

    ForecastId INT NOT NULL,

    Month INT NOT NULL,

    Year INT NOT NULL,

    AssignedMWh DECIMAL(18,4) NOT NULL,

    UnitPrice DECIMAL(18,4) NOT NULL,

    TaxPercentage DECIMAL(18,4) NOT NULL,

    Subtotal DECIMAL(18,4) NOT NULL,

    TaxAmount DECIMAL(18,4) NOT NULL,

    Total DECIMAL(18,4) NOT NULL,

    Status VARCHAR(50) NOT NULL,

    RevisionNumber INT NOT NULL DEFAULT(0),

    ParentId INT NULL,

    AnnulmentReason VARCHAR(500) NULL,

    IssueDate DATETIME2 NOT NULL,

    Created DATETIME2 NOT NULL DEFAULT(GETDATE()),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_AccountStatement_User
        FOREIGN KEY(BuyerId)
        REFERENCES tblUsers(Id),

    CONSTRAINT FK_AccountStatement_Distribution
        FOREIGN KEY(DistributionId)
        REFERENCES tblCommercialDistribution(Id),

    CONSTRAINT FK_AccountStatement_Forecast
        FOREIGN KEY(ForecastId)
        REFERENCES tblForecast(Id),

    CONSTRAINT FK_AccountStatement_Parent
        FOREIGN KEY(ParentId)
        REFERENCES tblAccountStatement(Id)
);
GO

CREATE TABLE tblNotificationQueue
(
    Id INT IDENTITY PRIMARY KEY,

    UserId INT NOT NULL,

    RecipientEmail VARCHAR(250) NOT NULL,

    NotificationType VARCHAR(40) NOT NULL,

    Subject VARCHAR(250) NOT NULL,

    Body NVARCHAR(MAX) NOT NULL,

    IsCritical BIT NOT NULL,

    Status VARCHAR(10) NOT NULL,

    Attempts INT NOT NULL DEFAULT 0,

    NextAttempt DATETIME2 NULL,

    SentDate DATETIME2 NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_NotificationQueue_User
        FOREIGN KEY(UserId)
        REFERENCES tblUsers(Id)
);

CREATE TABLE tblAuditLog
(
    Id INT IDENTITY PRIMARY KEY,

    UserId INT NULL,

    UserName VARCHAR(200) NOT NULL,

    Module VARCHAR(30) NOT NULL,

    Action VARCHAR(20) NOT NULL,

    AffectedEntity VARCHAR(100) NOT NULL,

    EntityId INT NOT NULL,

    PreviousValue NVARCHAR(MAX) NULL,

    NewValue NVARCHAR(MAX) NULL,

    EventDate DATETIME2 NOT NULL,

    IsColdArchive BIT NOT NULL DEFAULT 0,

    Created DATETIME2 NOT NULL DEFAULT GETDATE(),

    Updated DATETIME2 NULL,

    CONSTRAINT FK_AuditLog_User
        FOREIGN KEY(UserId)
        REFERENCES tblUsers(Id)
);

CREATE TABLE tblExportLog
(
    Id INT IDENTITY PRIMARY KEY,

    UserId INT NOT NULL,

    DocumentType VARCHAR(50) NOT NULL,

    DocumentId INT NOT NULL,

    Format VARCHAR(5) NOT NULL,

    FilePath VARCHAR(500) NOT NULL,

    Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    UpdatedAt DATETIME2 NULL,


    CONSTRAINT FK_ExportLog_User
        FOREIGN KEY(UserId)
        REFERENCES tblUsers(Id)
);

