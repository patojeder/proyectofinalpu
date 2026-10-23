USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
    THROW 50001, 'Ya existen tablas de la aplicación. Usar un script de actualización.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.Roles (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        Name NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_Name UNIQUE,
        IsCustom BIT NOT NULL CONSTRAINT DF_Roles_IsCustom DEFAULT (0)
    );

    CREATE TABLE dbo.Permissions (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Permissions PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL CONSTRAINT UQ_Permissions_Name UNIQUE,
        Description NVARCHAR(255) NULL
    );

    CREATE TABLE dbo.RolePermissions (
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL,
        CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
        CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(Id) ON DELETE CASCADE
    );

    CREATE TABLE dbo.Users (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        RoleId INT NOT NULL,
        UserName NVARCHAR(50) NOT NULL CONSTRAINT UQ_Users_UserName UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id)
    );

    CREATE INDEX IX_Users_RoleId ON dbo.Users(RoleId);

    CREATE TABLE dbo.Categories (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL CONSTRAINT UQ_Categories_Name UNIQUE,
        Description NVARCHAR(MAX) NULL,
        CONSTRAINT CK_Categories_Name CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
    );

    CREATE TABLE dbo.Services (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Services PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL CONSTRAINT UQ_Services_Name UNIQUE,
        Description NVARCHAR(MAX) NULL,
        CONSTRAINT CK_Services_Name CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
    );

    CREATE TABLE dbo.ServiceCategories (
        ServiceId INT NOT NULL,
        CategoryId INT NOT NULL,
        CONSTRAINT PK_ServiceCategories PRIMARY KEY (ServiceId, CategoryId),
        CONSTRAINT FK_ServiceCategories_Services FOREIGN KEY (ServiceId) REFERENCES dbo.Services(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ServiceCategories_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
    );

    CREATE INDEX IX_ServiceCategories_CategoryId ON dbo.ServiceCategories(CategoryId);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
