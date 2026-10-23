USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.ProductStocks', N'U') IS NULL OR OBJECT_ID(N'dbo.Users', N'U') IS NULL
    THROW 50001, 'Primero crear las tablas de productos, stock y usuarios.', 1;

IF OBJECT_ID(N'dbo.StockMovements') IS NOT NULL
    THROW 50002, 'Ya existe el historial de stock. No se modificó.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.StockMovements (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockMovements PRIMARY KEY,
        ProductId INT NOT NULL,
        UserId INT NULL,
        UserName NVARCHAR(50) NOT NULL,
        MovementType VARCHAR(20) NOT NULL,
        QuantityChange BIGINT NOT NULL,
        QuantityBefore INT NOT NULL,
        QuantityAfter INT NOT NULL,
        Reason NVARCHAR(500) NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_StockMovements_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        OperationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT UQ_StockMovements_OperationId UNIQUE,
        CONSTRAINT FK_StockMovements_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
        CONSTRAINT FK_StockMovements_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE SET NULL,
        CONSTRAINT CK_StockMovements_Quantity CHECK (
            QuantityChange <> 0 AND CONVERT(BIGINT, QuantityBefore) + QuantityChange = QuantityAfter
        ),
        CONSTRAINT CK_StockMovements_Type CHECK (
            (MovementType = 'Receipt' AND QuantityChange > 0)
            OR (MovementType = 'Adjustment' AND QuantityAfter >= 0 AND LEN(LTRIM(RTRIM(Reason))) > 0 AND Reason IS NOT NULL)
            OR MovementType = 'OpeningBalance'
        )
    );

    CREATE INDEX IX_StockMovements_ProductId_CreatedAtUtc ON dbo.StockMovements(ProductId, CreatedAtUtc DESC, Id DESC);
    CREATE INDEX IX_StockMovements_UserId ON dbo.StockMovements(UserId);

    INSERT INTO dbo.StockMovements
        (ProductId, UserId, UserName, MovementType, QuantityChange, QuantityBefore, QuantityAfter, Reason, OperationId)
    SELECT ProductId, NULL, N'Sistema', 'OpeningBalance', CONVERT(BIGINT, Quantity), 0, Quantity,
        N'Saldo existente antes de incorporar el historial de stock.', NEWID()
    FROM dbo.ProductStocks WITH (TABLOCKX, HOLDLOCK)
    WHERE Quantity <> 0;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
