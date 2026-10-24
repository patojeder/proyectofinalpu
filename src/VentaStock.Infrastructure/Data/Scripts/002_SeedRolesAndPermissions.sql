USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
    OR OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
    OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
    THROW 50001, 'Ejecutar primero 001_CreateSchema.sql o actualizar el esquema a inglés.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Roles TABLE (Name NVARCHAR(50) PRIMARY KEY);
    INSERT INTO @Roles VALUES (N'Dueño'), (N'Gestor de Usuarios'), (N'Gestor de Productos'), (N'Gestor de Stock'), (N'Gestor de Precios'), (N'Gestor de Clientes'), (N'Vendedor');

    IF EXISTS (
        SELECT 1 FROM dbo.Roles r INNER JOIN @Roles n ON n.Name = r.Name
        WHERE r.IsCustom = 1
    )
        THROW 50002, 'Un nombre de rol reservado está asignado a un rol personalizado.', 1;

    INSERT INTO dbo.Roles (Name, IsCustom)
    SELECT n.Name, 0 FROM @Roles n
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.Name = n.Name);

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
        (N'Services.Read', N'Consultar servicios.'),
        (N'Services.Create', N'Crear servicios.'),
        (N'Services.Update', N'Modificar servicios.'),
        (N'Services.Delete', N'Eliminar servicios.'),
        (N'Products.Read', N'Consultar productos.'),
        (N'Products.Create', N'Crear productos.'),
        (N'Products.Update', N'Modificar productos.'),
        (N'Products.Delete', N'Eliminar productos.'),
        (N'Products.ReadCost', N'Consultar costo de productos.'),
        (N'Products.ReadSalePrice', N'Consultar precio de venta de productos.'),
        (N'Products.UpdatePrices', N'Asignar o modificar precios de productos.'),
        (N'Stock.Read', N'Consultar existencias de productos.'),
        (N'Stock.Receipt', N'Registrar ingresos de mercadería.'),
        (N'Stock.Adjust', N'Ajustar existencias por conteo físico.'),
        (N'Stock.ReadMovements', N'Consultar el historial de movimientos de stock.'),
        (N'Customers.Read', N'Consultar clientes.'),
        (N'Customers.Create', N'Crear clientes de contado.'),
        (N'Customers.CreateOnAccount', N'Crear clientes de cuenta corriente o habilitar ese tipo.'),
        (N'Customers.Update', N'Modificar clientes.'),
        (N'Sales.Read', N'Consultar ventas.'),
        (N'Sales.Create', N'Realizar ventas.');

    INSERT INTO dbo.Permissions (Name, Description)
    SELECT n.Name, n.Description FROM @Permissions n
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions p WHERE p.Name = n.Name);

    INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
    SELECT r.Id, p.Id
    FROM dbo.Roles r
    CROSS JOIN dbo.Permissions p
    INNER JOIN @Permissions n ON n.Name = p.Name
    WHERE r.IsCustom = 0 AND (
        r.Name = N'Dueño'
        OR (r.Name = N'Gestor de Usuarios' AND p.Name LIKE N'Users.%')
        OR (r.Name = N'Gestor de Productos' AND (p.Name LIKE N'Categories.%' OR p.Name LIKE N'Services.%' OR p.Name LIKE N'Products.%' OR p.Name = N'Stock.Read'))
        OR (r.Name = N'Gestor de Stock' AND (p.Name LIKE N'Stock.%' OR p.Name IN (N'Products.Read', N'Products.Create', N'Categories.Read')))
        OR (r.Name = N'Gestor de Precios' AND p.Name IN (N'Products.Read', N'Products.ReadCost', N'Products.ReadSalePrice', N'Products.UpdatePrices', N'Categories.Read'))
        OR (r.Name = N'Gestor de Clientes' AND p.Name LIKE N'Customers.%')
        OR (r.Name = N'Vendedor' AND p.Name IN (N'Sales.Read', N'Sales.Create', N'Customers.Read', N'Customers.Create', N'Categories.Read', N'Services.Read', N'Products.Read', N'Products.ReadSalePrice', N'Stock.Read'))
    ) AND NOT EXISTS (
        SELECT 1 FROM dbo.RolePermissions rp
        WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
