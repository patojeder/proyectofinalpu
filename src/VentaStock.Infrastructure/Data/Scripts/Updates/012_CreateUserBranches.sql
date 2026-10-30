USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'VentaStockDb'
    THROW 51000, 'El archivo solo puede ejecutarse sobre VentaStockDb.', 1;

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    OR OBJECT_ID(N'dbo.Roles', N'U') IS NULL
    OR OBJECT_ID(N'dbo.Branches', N'U') IS NULL
    THROW 51001, 'Primero crear las tablas de usuarios, roles y sucursales.', 1;

IF OBJECT_ID(N'dbo.UserBranches', N'U') IS NOT NULL
    THROW 51002, 'Ya existe la tabla dbo.UserBranches. No se realizaron cambios.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.UserBranches (
        UserId INT NOT NULL,
        BranchId INT NOT NULL,
        CONSTRAINT PK_UserBranches PRIMARY KEY (UserId, BranchId),
        CONSTRAINT FK_UserBranches_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserBranches_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_UserBranches_BranchId ON dbo.UserBranches(BranchId);

    DECLARE @DefaultBranchId INT = (SELECT TOP (1) Id FROM dbo.Branches ORDER BY Id);

    IF @DefaultBranchId IS NOT NULL
    BEGIN
        INSERT INTO dbo.UserBranches (UserId, BranchId)
        SELECT u.Id, @DefaultBranchId
        FROM dbo.Users u
        INNER JOIN dbo.Roles r ON r.Id = u.RoleId
        WHERE r.Name <> N'Dueño'
          AND NOT EXISTS (
              SELECT 1 FROM dbo.UserBranches ub
              WHERE ub.UserId = u.Id AND ub.BranchId = @DefaultBranchId
          );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

