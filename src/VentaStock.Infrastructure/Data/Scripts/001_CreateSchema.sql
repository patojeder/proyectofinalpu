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

    CREATE TABLE dbo.Customers (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        DocumentType VARCHAR(4) NOT NULL,
        DocumentNumber VARCHAR(11) NOT NULL CONSTRAINT UQ_Customers_DocumentNumber UNIQUE,
        CustomerType VARCHAR(10) NOT NULL,
        Address NVARCHAR(250) NULL,
        Phone NVARCHAR(50) NULL,
        Version ROWVERSION NOT NULL,
        CONSTRAINT CK_Customers_FirstName CHECK (LEN(LTRIM(RTRIM(FirstName))) > 0),
        CONSTRAINT CK_Customers_LastName CHECK (LEN(LTRIM(RTRIM(LastName))) > 0),
        CONSTRAINT CK_Customers_Document CHECK (
            DocumentNumber NOT LIKE '%[^0-9]%' COLLATE Latin1_General_100_BIN2
            AND ((DocumentType = 'DNI' AND LEN(DocumentNumber) = 8 AND DocumentNumber <> '00000000')
                OR (DocumentType IN ('CUIL', 'CUIT') AND LEN(DocumentNumber) = 11))
        ),
        CONSTRAINT CK_Customers_Type CHECK (CustomerType IN ('Cash', 'OnAccount')),
        CONSTRAINT CK_Customers_OnAccountDocument CHECK (CustomerType <> 'OnAccount' OR DocumentType IN ('CUIL', 'CUIT'))
    );

    CREATE INDEX IX_Customers_LastName_FirstName ON dbo.Customers(LastName, FirstName);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
