USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.rol', N'U') IS NULL
    OR OBJECT_ID(N'dbo.permiso', N'U') IS NULL
    OR OBJECT_ID(N'dbo.rol_permiso', N'U') IS NULL
    THROW 50001, 'Ejecutar primero 001_CreateSchema.sql.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Roles TABLE (Name NVARCHAR(50) PRIMARY KEY);
    INSERT INTO @Roles VALUES (N'Owner'), (N'UserManager'), (N'ProductManager'), (N'Vendedor');

    IF EXISTS (
        SELECT 1 FROM dbo.rol r INNER JOIN @Roles n ON n.Name = r.nombre
        WHERE r.es_personalizado = 1
    )
        THROW 50002, 'Un nombre de rol reservado está asignado a un rol personalizado.', 1;

    INSERT INTO dbo.rol (nombre, es_personalizado)
    SELECT n.Name, 0 FROM @Roles n
    WHERE NOT EXISTS (SELECT 1 FROM dbo.rol r WHERE r.nombre = n.Name);

    DECLARE @Permissions TABLE (Name NVARCHAR(100) PRIMARY KEY, Description NVARCHAR(255));
    INSERT INTO @Permissions VALUES
        (N'Users.Read', N'Consultar usuarios.'),
        (N'Users.Create', N'Crear usuarios.'),
        (N'Users.Update', N'Modificar usuarios.'),
        (N'Users.Delete', N'Eliminar usuarios.'),
        (N'Users.ResetPassword', N'Restablecer contraseñas.'),
        (N'Categories.Read', N'Consultar categorías.'),
        (N'Categories.Create', N'Crear categorías.'),
        (N'Categories.Update', N'Modificar categorías.'),
        (N'Categories.Delete', N'Eliminar categorías.'),
        (N'Sales.Read', N'Consultar ventas.'),
        (N'Sales.Create', N'Realizar ventas.');

    INSERT INTO dbo.permiso (nombre, descripcion)
    SELECT n.Name, n.Description FROM @Permissions n
    WHERE NOT EXISTS (SELECT 1 FROM dbo.permiso p WHERE p.nombre = n.Name);

    INSERT INTO dbo.rol_permiso (id_rol, id_permiso)
    SELECT r.id_rol, p.id_permiso
    FROM dbo.rol r
    CROSS JOIN dbo.permiso p
    INNER JOIN @Permissions n ON n.Name = p.nombre
    WHERE r.es_personalizado = 0 AND (
        r.nombre = N'Owner'
        OR (r.nombre = N'UserManager' AND p.nombre LIKE N'Users.%')
        OR (r.nombre = N'ProductManager' AND p.nombre LIKE N'Categories.%')
        OR (r.nombre = N'Vendedor' AND p.nombre IN (N'Sales.Read', N'Sales.Create', N'Categories.Read'))
    ) AND NOT EXISTS (
        SELECT 1 FROM dbo.rol_permiso rp
        WHERE rp.id_rol = r.id_rol AND rp.id_permiso = p.id_permiso
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
