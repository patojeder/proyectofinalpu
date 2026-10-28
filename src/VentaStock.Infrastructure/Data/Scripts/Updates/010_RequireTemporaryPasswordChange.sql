USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'VentaStockDb'
    THROW 51000, 'El archivo solo puede ejecutarse sobre VentaStockDb.', 1;

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    THROW 51001, 'No existe dbo.Users. Crear o actualizar primero el esquema.', 1;

IF COL_LENGTH(N'dbo.Users', N'MustChangePassword') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD MustChangePassword BIT NOT NULL
            CONSTRAINT DF_Users_MustChangePassword DEFAULT (0) WITH VALUES;
END;
GO
