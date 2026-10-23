USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
    THROW 50001, 'Primero actualizar el esquema a inglés.', 1;

IF OBJECT_ID(N'dbo.Services') IS NOT NULL OR OBJECT_ID(N'dbo.ServiceCategories') IS NOT NULL
    THROW 50002, 'Ya existen tablas de servicios. No se modificaron.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

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
