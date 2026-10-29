USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'VentaStockDb'
    THROW 51000, 'El archivo solo puede ejecutarse sobre VentaStockDb.', 1;

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
    OR OBJECT_ID(N'dbo.ProductStocks', N'U') IS NULL
    OR OBJECT_ID(N'dbo.StockMovements', N'U') IS NULL
    THROW 51001, 'Primero crear las tablas de productos, stock y movimientos.', 1;

IF OBJECT_ID(N'dbo.Branches', N'U') IS NOT NULL
    THROW 51002, 'Ya existe la tabla dbo.Branches. No se realizaron cambios.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.Branches (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Branches PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL CONSTRAINT UQ_Branches_Name UNIQUE,
        Address NVARCHAR(250) NULL,
        Phone NVARCHAR(50) NULL,
        CONSTRAINT CK_Branches_Name CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
    );

    DECLARE @DefaultBranchId INT = NULL;

    IF EXISTS (SELECT 1 FROM dbo.Products) OR EXISTS (SELECT 1 FROM dbo.StockMovements)
    BEGIN
        INSERT INTO dbo.Branches (Name, Address, Phone)
        VALUES (N'Casa Central', NULL, NULL);

        SET @DefaultBranchId = SCOPE_IDENTITY();
    END;

    CREATE TABLE #OldProductStocks (
        ProductId INT NOT NULL PRIMARY KEY,
        Quantity INT NOT NULL,
        MinimumQuantity INT NOT NULL
    );

    INSERT INTO #OldProductStocks (ProductId, Quantity, MinimumQuantity)
    SELECT ProductId, Quantity, MinimumQuantity
    FROM dbo.ProductStocks;

    DROP TABLE dbo.ProductStocks;

    CREATE TABLE dbo.ProductStocks (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductStocks PRIMARY KEY,
        ProductId INT NOT NULL,
        BranchId INT NOT NULL,
        Quantity INT NOT NULL CONSTRAINT DF_ProductStocks_Quantity DEFAULT (0),
        MinimumQuantity INT NOT NULL CONSTRAINT DF_ProductStocks_MinimumQuantity DEFAULT (0),
        Sector NVARCHAR(50) NULL,
        Shelf NVARCHAR(50) NULL,
        CONSTRAINT UQ_ProductStocks_ProductId_BranchId UNIQUE (ProductId, BranchId),
        CONSTRAINT CK_ProductStocks_MinimumQuantity CHECK (MinimumQuantity >= 0),
        CONSTRAINT FK_ProductStocks_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProductStocks_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ProductStocks_BranchId ON dbo.ProductStocks(BranchId);

    IF @DefaultBranchId IS NOT NULL
    BEGIN
        EXEC sys.sp_executesql N'
            INSERT INTO dbo.ProductStocks (ProductId, BranchId, Quantity, MinimumQuantity, Sector, Shelf)
            SELECT p.Id, @BranchId, ISNULL(ps.Quantity, 0), ISNULL(ps.MinimumQuantity, 0), p.Sector, p.Shelf
            FROM dbo.Products p
            LEFT JOIN #OldProductStocks ps ON ps.ProductId = p.Id;',
            N'@BranchId INT',
            @BranchId = @DefaultBranchId;
    END;

    DROP TABLE #OldProductStocks;

    IF COL_LENGTH(N'dbo.Products', N'Sector') IS NOT NULL
        ALTER TABLE dbo.Products DROP COLUMN Sector;

    IF COL_LENGTH(N'dbo.Products', N'Shelf') IS NOT NULL
        ALTER TABLE dbo.Products DROP COLUMN Shelf;

    ALTER TABLE dbo.StockMovements ADD BranchId INT NULL;

    IF @DefaultBranchId IS NOT NULL
    BEGIN
        EXEC sys.sp_executesql
            N'UPDATE dbo.StockMovements SET BranchId = @BranchId WHERE BranchId IS NULL;',
            N'@BranchId INT',
            @BranchId = @DefaultBranchId;
    END;

    ALTER TABLE dbo.StockMovements ALTER COLUMN BranchId INT NOT NULL;

    ALTER TABLE dbo.StockMovements
        ADD CONSTRAINT FK_StockMovements_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id);

    IF EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID(N'dbo.StockMovements')
          AND name = N'CK_StockMovements_Type'
    )
        ALTER TABLE dbo.StockMovements DROP CONSTRAINT CK_StockMovements_Type;

    ALTER TABLE dbo.StockMovements
        ADD CONSTRAINT CK_StockMovements_Type CHECK (
            (MovementType = 'Receipt' AND QuantityChange > 0)
            OR (MovementType = 'Adjustment' AND QuantityAfter >= 0 AND LEN(LTRIM(RTRIM(Reason))) > 0 AND Reason IS NOT NULL)
            OR (MovementType = 'Sale' AND QuantityChange < 0)
            OR MovementType = 'OpeningBalance'
        );

    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.StockMovements')
          AND name = N'IX_StockMovements_ProductId_CreatedAtUtc'
    )
        DROP INDEX IX_StockMovements_ProductId_CreatedAtUtc ON dbo.StockMovements;

    CREATE INDEX IX_StockMovements_ProductId_BranchId_CreatedAtUtc
        ON dbo.StockMovements(ProductId, BranchId, CreatedAtUtc DESC, Id DESC);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
