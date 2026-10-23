USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
    THROW 50001, 'Primero crear el esquema de categorías.', 1;

IF OBJECT_ID(N'dbo.Products') IS NOT NULL
    OR OBJECT_ID(N'dbo.ProductCategories') IS NOT NULL
    OR OBJECT_ID(N'dbo.ProductStocks') IS NOT NULL
    THROW 50002, 'Ya existen tablas de productos. No se modificaron.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.Products (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Name NVARCHAR(150) NOT NULL CONSTRAINT UQ_Products_Name UNIQUE,
        Description NVARCHAR(MAX) NULL,
        Barcode NVARCHAR(100) NOT NULL
            CONSTRAINT DF_Products_Barcode DEFAULT (N'INT-' + REPLACE(CONVERT(NVARCHAR(36), NEWID()), N'-', N''))
            CONSTRAINT UQ_Products_Barcode UNIQUE,
        Sector NVARCHAR(50) NULL,
        Shelf NVARCHAR(50) NULL,
        CostPrice DECIMAL(18,2) NULL,
        SalePrice DECIMAL(18,2) NULL,
        CONSTRAINT CK_Products_Name CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
        CONSTRAINT CK_Products_Barcode CHECK (LEN(LTRIM(RTRIM(Barcode))) > 0),
        CONSTRAINT CK_Products_Prices CHECK (
            (CostPrice IS NULL AND SalePrice IS NULL)
            OR (CostPrice IS NOT NULL AND SalePrice IS NOT NULL AND CostPrice >= 0 AND SalePrice > CostPrice)
        )
    );

    CREATE TABLE dbo.ProductCategories (
        ProductId INT NOT NULL,
        CategoryId INT NOT NULL,
        CONSTRAINT PK_ProductCategories PRIMARY KEY (ProductId, CategoryId),
        CONSTRAINT FK_ProductCategories_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProductCategories_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id)
    );

    CREATE INDEX IX_ProductCategories_CategoryId ON dbo.ProductCategories(CategoryId);

    CREATE TABLE dbo.ProductStocks (
        ProductId INT NOT NULL CONSTRAINT PK_ProductStocks PRIMARY KEY,
        Quantity INT NOT NULL CONSTRAINT DF_ProductStocks_Quantity DEFAULT (0),
        MinimumQuantity INT NOT NULL CONSTRAINT DF_ProductStocks_MinimumQuantity DEFAULT (0),
        CONSTRAINT CK_ProductStocks_MinimumQuantity CHECK (MinimumQuantity >= 0),
        CONSTRAINT FK_ProductStocks_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
