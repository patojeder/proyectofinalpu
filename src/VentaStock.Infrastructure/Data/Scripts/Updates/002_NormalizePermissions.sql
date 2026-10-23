-- Solo para bases existentes con permisos anteriores; conserva las asignaciones de los permisos renombrados.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 50001, 'Seleccioná la base de la aplicación.', 1;

IF OBJECT_ID(N'dbo.rol', N'U') IS NULL
    OR OBJECT_ID(N'dbo.permiso', N'U') IS NULL
    OR OBJECT_ID(N'dbo.rol_permiso', N'U') IS NULL
    THROW 50002, 'Deben existir las tablas de roles y permisos.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.rol WHERE nombre = N'Administrador')
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.rol WHERE nombre = N'Owner')
            THROW 50003, 'Existen Administrador y Owner. Resolver sus cuentas antes de continuar.', 1;
        IF EXISTS (SELECT 1 FROM dbo.rol WHERE nombre = N'Administrador' AND es_personalizado = 1)
            THROW 50004, 'Administrador es un rol personalizado; revisar antes de renombrarlo.', 1;
        UPDATE dbo.rol SET nombre = N'Owner' WHERE nombre = N'Administrador';
    END;

    IF EXISTS (SELECT nombre FROM dbo.permiso GROUP BY nombre HAVING COUNT(*) > 1)
        THROW 50005, 'Hay nombres de permisos duplicados. Resolverlos antes de continuar.', 1;

    DECLARE @Names TABLE (OldName NVARCHAR(100) PRIMARY KEY, NewName NVARCHAR(100));
    INSERT INTO @Names VALUES
        (N'VENTAS_CREAR', N'Sales.Create'),
        (N'VENTAS_VER', N'Sales.Read'),
        (N'Users.Deactivate', N'Users.Delete'),
        (N'Categories.Deactivate', N'Categories.Delete');

    INSERT INTO dbo.permiso (nombre, descripcion)
    SELECT n.NewName, p.descripcion
    FROM @Names n INNER JOIN dbo.permiso p ON p.nombre = n.OldName
    WHERE NOT EXISTS (SELECT 1 FROM dbo.permiso target WHERE target.nombre = n.NewName);

    INSERT INTO dbo.rol_permiso (id_rol, id_permiso)
    SELECT DISTINCT rp.id_rol, target.id_permiso
    FROM dbo.rol_permiso rp
    INNER JOIN dbo.permiso source ON source.id_permiso = rp.id_permiso
    INNER JOIN @Names n ON n.OldName = source.nombre
    INNER JOIN dbo.permiso target ON target.nombre = n.NewName
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.rol_permiso existing
        WHERE existing.id_rol = rp.id_rol AND existing.id_permiso = target.id_permiso
    );

    DELETE rp
    FROM dbo.rol_permiso rp INNER JOIN dbo.permiso p ON p.id_permiso = rp.id_permiso
    WHERE p.nombre IN (SELECT OldName FROM @Names)
        OR p.nombre IN (N'USUARIOS_ADMIN', N'STOCK_ADMIN');

    DELETE FROM dbo.permiso
    WHERE nombre IN (SELECT OldName FROM @Names)
        OR nombre IN (N'USUARIOS_ADMIN', N'STOCK_ADMIN');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
