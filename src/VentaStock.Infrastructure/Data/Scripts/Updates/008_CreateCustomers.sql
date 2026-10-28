USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Customers') IS NOT NULL
    THROW 50001, 'Ya existe la tabla de clientes. No se modificó.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

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
