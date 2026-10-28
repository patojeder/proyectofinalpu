USE [VentaStockDb];

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Datos exclusivamente locales para pruebas. No ejecutar en producción.
-- Usuario / contraseña:
-- Dueño / Dueño123
-- GestorUsuarios / GestorUsuarios123
-- GestorProductos / GestorProductos123
-- GestorStock / GestorStock123
-- GestorPrecios / GestorPrecios123
-- GestorClientes / GestorClientes123
-- Vendedor / Vendedor123

IF DB_NAME() <> N'VentaStockDb'
    THROW 51000, 'El archivo solo puede ejecutarse sobre VentaStockDb.', 1;

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    OR OBJECT_ID(N'dbo.Roles', N'U') IS NULL
    OR OBJECT_ID(N'dbo.Categories', N'U') IS NULL
    OR OBJECT_ID(N'dbo.Services', N'U') IS NULL
    OR OBJECT_ID(N'dbo.Products', N'U') IS NULL
    OR OBJECT_ID(N'dbo.Customers', N'U') IS NULL
    THROW 51001, 'Primero crear y actualizar el esquema de VentaStock.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @TestAccounts TABLE
    (
        RoleName NVARCHAR(50) NOT NULL PRIMARY KEY,
        UserName NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL
    );

    INSERT INTO @TestAccounts (RoleName, UserName, PasswordHash)
    VALUES
        (N'Dueño', N'Dueño', N'$2a$11$OSYdYIhRJl6Xhn4voeoHBuVZyZn0Vunm6OJcjs1vN81oi0MQM7olS'),
        (N'Gestor de Usuarios', N'GestorUsuarios', N'$2a$11$ggRdYel7rbBGK7BnOwCLXOGExB2jnqkeA5JFVacOx2vCELX8geZUK'),
        (N'Gestor de Productos', N'GestorProductos', N'$2a$11$zCWclSHHbkvRcLbFSLE0Ueao/sXqYeTG/qkRfhw2vNGhWMU99v3pK'),
        (N'Gestor de Stock', N'GestorStock', N'$2a$11$l5bzglpYx/ZmUWI1rD94z.LRl2zixo7ypqhlZ8YT/uIbVjGKRQ/dO'),
        (N'Gestor de Precios', N'GestorPrecios', N'$2a$11$.S8OTuDR0ZiAKt3TvShuGeVEbFLWCIElrl2TWF3rPWEIV6V1e6Jne'),
        (N'Gestor de Clientes', N'GestorClientes', N'$2a$11$TSBT.4kuO7nrkkdpyjseU.RY9Rw268IOS.NurG2oClSd7T3wcmQ62'),
        (N'Vendedor', N'Vendedor', N'$2a$11$Ft2yvkZ2V9yc2koaXVmwJuFadbpo.EKf/hNs5IeflRLVM60X8gfBK');

    IF (SELECT COUNT(*) FROM dbo.Roles WHERE Name IN
        (N'Dueño', N'Gestor de Usuarios', N'Gestor de Productos', N'Gestor de Stock', N'Gestor de Precios', N'Gestor de Clientes', N'Vendedor')) <> 7
        THROW 51002, 'Faltan roles base. Ejecutar primero los scripts de esquema y permisos.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Users u
        INNER JOIN dbo.Roles r ON r.Id = u.RoleId
        INNER JOIN @TestAccounts t ON t.RoleName = r.Name
        GROUP BY r.Name
        HAVING COUNT(*) > 1
    )
        THROW 51003, 'Hay más de una cuenta para algún rol base. Revisar antes de cargar los datos.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Users u
        INNER JOIN @TestAccounts t ON t.UserName = u.UserName
        INNER JOIN dbo.Roles r ON r.Id = u.RoleId
        WHERE r.Name <> t.RoleName
    )
        THROW 51004, 'Uno de los nombres de prueba ya pertenece a otro rol.', 1;

    UPDATE u
       SET u.UserName = t.UserName,
           u.PasswordHash = t.PasswordHash,
           u.MustChangePassword = 0
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON r.Id = u.RoleId
    INNER JOIN @TestAccounts t ON t.RoleName = r.Name;

    INSERT INTO dbo.Users (RoleId, UserName, PasswordHash, MustChangePassword, CreatedAtUtc)
    SELECT r.Id, t.UserName, t.PasswordHash, 0, SYSUTCDATETIME()
    FROM @TestAccounts t
    INNER JOIN dbo.Roles r ON r.Name = t.RoleName
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.RoleId = r.Id);

    INSERT INTO dbo.Categories (Name, Description)
    SELECT v.Name, v.Description
    FROM (VALUES
        (N'Papelería de prueba', N'Categoría para pruebas de productos y servicios.'),
        (N'Accesorios de prueba', N'Categoría para probar productos y filtros.'),
        (N'Categoría libre de prueba', N'Categoría sin relaciones para probar su eliminación.')) v(Name, Description)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories c WHERE c.Name = v.Name);

    INSERT INTO dbo.Services (Name, Description)
    SELECT v.Name, v.Description
    FROM (VALUES
        (N'Anillado de prueba', N'Servicio de ejemplo para verificar el ABM y su relación con categorías.'),
        (N'Impresión color de prueba', N'Servicio de ejemplo sin categoría asociada.')) v(Name, Description)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Services s WHERE s.Name = v.Name);

    INSERT INTO dbo.ServiceCategories (ServiceId, CategoryId)
    SELECT s.Id, c.Id
    FROM dbo.Services s
    CROSS JOIN dbo.Categories c
    WHERE s.Name = N'Anillado de prueba'
      AND c.Name = N'Papelería de prueba'
      AND NOT EXISTS
      (
          SELECT 1 FROM dbo.ServiceCategories sc
          WHERE sc.ServiceId = s.Id AND sc.CategoryId = c.Id
      );

    INSERT INTO dbo.Products (Name, Description, Barcode, Sector, Shelf, CostPrice, SalePrice)
    SELECT v.Name, v.Description, v.Barcode, v.Sector, v.Shelf, v.CostPrice, v.SalePrice
    FROM (VALUES
        (N'Cuaderno de prueba', N'Producto de prueba para categorías, precios y stock.', N'TEST-CUADERNO-001', N'Prueba', N'A1', CONVERT(DECIMAL(18,2), 1000), CONVERT(DECIMAL(18,2), 1500)),
        (N'Lapicera azul de prueba', N'Producto de prueba adicional para búsquedas y stock bajo.', N'TEST-LAPICERA-001', N'Prueba', N'A2', CONVERT(DECIMAL(18,2), 500), CONVERT(DECIMAL(18,2), 800))) v(Name, Description, Barcode, Sector, Shelf, CostPrice, SalePrice)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Products p WHERE p.Name = v.Name);

    UPDATE p
       SET p.Description = v.Description,
           p.Sector = v.Sector,
           p.Shelf = v.Shelf,
           p.CostPrice = v.CostPrice,
           p.SalePrice = v.SalePrice
    FROM dbo.Products p
    INNER JOIN (VALUES
        (N'Cuaderno de prueba', N'Producto de prueba para categorías, precios y stock.', N'Prueba', N'A1', CONVERT(DECIMAL(18,2), 1000), CONVERT(DECIMAL(18,2), 1500)),
        (N'Lapicera azul de prueba', N'Producto de prueba adicional para búsquedas y stock bajo.', N'Prueba', N'A2', CONVERT(DECIMAL(18,2), 500), CONVERT(DECIMAL(18,2), 800))) v(Name, Description, Sector, Shelf, CostPrice, SalePrice)
        ON p.Name = v.Name;

    INSERT INTO dbo.ProductCategories (ProductId, CategoryId)
    SELECT p.Id, c.Id
    FROM (VALUES
        (N'Cuaderno de prueba', N'Papelería de prueba'),
        (N'Lapicera azul de prueba', N'Accesorios de prueba')) v(ProductName, CategoryName)
    INNER JOIN dbo.Products p ON p.Name = v.ProductName
    INNER JOIN dbo.Categories c ON c.Name = v.CategoryName
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.ProductCategories pc
        WHERE pc.ProductId = p.Id AND pc.CategoryId = c.Id
    );

    INSERT INTO dbo.ProductStocks (ProductId, Quantity, MinimumQuantity)
    SELECT p.Id, 0, v.MinimumQuantity
    FROM (VALUES
        (N'Cuaderno de prueba', 3),
        (N'Lapicera azul de prueba', 5)) v(ProductName, MinimumQuantity)
    INNER JOIN dbo.Products p ON p.Name = v.ProductName
    WHERE NOT EXISTS (SELECT 1 FROM dbo.ProductStocks ps WHERE ps.ProductId = p.Id);

    UPDATE ps
       SET ps.MinimumQuantity = v.MinimumQuantity
    FROM dbo.ProductStocks ps
    INNER JOIN dbo.Products p ON p.Id = ps.ProductId
    INNER JOIN (VALUES
        (N'Cuaderno de prueba', 3),
        (N'Lapicera azul de prueba', 5)) v(ProductName, MinimumQuantity)
        ON p.Name = v.ProductName;

    INSERT INTO dbo.Customers (FirstName, LastName, DocumentType, DocumentNumber, CustomerType, Address, Phone)
    SELECT v.FirstName, v.LastName, v.DocumentType, v.DocumentNumber, v.CustomerType, v.Address, v.Phone
    FROM (VALUES
        (N'Cliente', N'Contado Prueba', 'DNI', '12345678', 'Cash', N'Calle de prueba 123', N'3810000001'),
        (N'Cliente', N'Cuenta Prueba', 'CUIT', '30123456781', 'OnAccount', N'Avenida de prueba 456', N'3810000002')) v(FirstName, LastName, DocumentType, DocumentNumber, CustomerType, Address, Phone)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Customers c WHERE c.DocumentNumber = v.DocumentNumber);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT N'Usuarios' AS Tipo, COUNT(*) AS Cantidad FROM dbo.Users
UNION ALL SELECT N'Categorías', COUNT(*) FROM dbo.Categories
UNION ALL SELECT N'Servicios', COUNT(*) FROM dbo.Services
UNION ALL SELECT N'Productos', COUNT(*) FROM dbo.Products
UNION ALL SELECT N'Clientes', COUNT(*) FROM dbo.Customers;
