USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
    THROW 50001, 'Primero actualizar los nombres del esquema a inglés.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Names TABLE (OldName NVARCHAR(50) PRIMARY KEY, NewName NVARCHAR(50));
    INSERT INTO @Names VALUES
        (N'Owner', N'Dueño'),
        (N'UserManager', N'Gestor de Usuarios'),
        (N'ProductManager', N'Gestor de Productos'),
        (N'Salesperson', N'Vendedor');

    IF EXISTS (
        SELECT 1 FROM dbo.Roles source
        INNER JOIN @Names n ON n.OldName = source.Name
        INNER JOIN dbo.Roles target ON target.Name = n.NewName AND target.Id <> source.Id
    )
        THROW 50002, 'Ya existe un rol con el nombre en español. Revisar antes de combinar cuentas o permisos.', 1;

    IF EXISTS (
        SELECT 1 FROM dbo.Roles r INNER JOIN @Names n ON n.OldName = r.Name
        WHERE r.IsCustom = 1
    )
        THROW 50003, 'Un nombre reservado está asignado a un rol personalizado. Revisarlo antes de renombrarlo.', 1;

    UPDATE r SET Name = n.NewName
    FROM dbo.Roles r INNER JOIN @Names n ON n.OldName = r.Name;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
