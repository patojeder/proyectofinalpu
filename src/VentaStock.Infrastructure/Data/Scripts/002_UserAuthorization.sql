-- Actualizar el esquema de prueba con nombres en español; no usar con el esquema en inglés.
-- Ejecutar en la base de la aplicación. No se eliminan tablas, usuarios ni contraseñas.
-- Después de aplicarlo, cerrar sesión y volver a ingresar para actualizar los claims.
-- Administrador pasa a llamarse Owner y se agrega UserManager.
-- La aplicación deja de aceptar el permiso USUARIOS_ADMIN.
SET XACT_ABORT ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 50001, 'Select the application database.', 1;

IF OBJECT_ID(N'dbo.rol', N'U') IS NULL OR OBJECT_ID(N'dbo.permiso', N'U') IS NULL
    OR OBJECT_ID(N'dbo.rol_permiso', N'U') IS NULL
    THROW 50002, 'This upgrade requires the current test schema.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.rol WHERE nombre = N'Administrador')
        AND EXISTS (SELECT 1 FROM dbo.rol WHERE nombre = N'Owner')
        THROW 50003, 'Both Administrador and Owner exist; resolve their identity before upgrading.', 1;

    IF EXISTS (SELECT 1 FROM dbo.rol WHERE nombre IN (N'Administrador', N'Owner', N'UserManager') AND es_personalizado = 1)
        THROW 50004, 'A reserved role name is currently assigned to a custom role.', 1;

    UPDATE dbo.rol SET nombre = N'Owner' WHERE nombre = N'Administrador' AND es_personalizado = 0;

    IF (SELECT COUNT(*) FROM dbo.rol WHERE nombre = N'Owner') <> 1
        THROW 50005, 'Exactly one Owner role is required.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.rol WHERE nombre = N'UserManager')
        INSERT INTO dbo.rol (nombre, es_personalizado) VALUES (N'UserManager', 0);

    IF (SELECT COUNT(*) FROM dbo.rol WHERE nombre = N'UserManager') <> 1
        THROW 50006, 'Exactly one UserManager role is required.', 1;

    DECLARE @Permissions TABLE (Name NVARCHAR(100), Description NVARCHAR(255));
    INSERT INTO @Permissions VALUES
        (N'Users.Read', N'View users.'),
        (N'Users.Create', N'Create users.'),
        (N'Users.Update', N'Update users.'),
        (N'Users.Deactivate', N'Deactivate users.'),
        (N'Users.ResetPassword', N'Reset user passwords.');

    IF EXISTS (
        SELECT p.nombre FROM dbo.permiso p JOIN @Permissions n ON p.nombre = n.Name
        GROUP BY p.nombre HAVING COUNT(*) > 1
    )
        THROW 50007, 'Duplicate user permission names exist.', 1;

    INSERT INTO dbo.permiso (nombre, descripcion)
    SELECT n.Name, n.Description FROM @Permissions n
    WHERE NOT EXISTS (SELECT 1 FROM dbo.permiso p WHERE p.nombre = n.Name);

    INSERT INTO dbo.rol_permiso (id_rol, id_permiso)
    SELECT r.id_rol, p.id_permiso
    FROM dbo.rol r CROSS JOIN dbo.permiso p
    JOIN @Permissions n ON p.nombre = n.Name
    WHERE r.nombre IN (N'Owner', N'UserManager') AND r.es_personalizado = 0
        AND NOT EXISTS (
            SELECT 1 FROM dbo.rol_permiso rp
            WHERE rp.id_rol = r.id_rol AND rp.id_permiso = p.id_permiso
        );

    -- Asignar permisos individuales explícitamente a los roles personalizados.
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
