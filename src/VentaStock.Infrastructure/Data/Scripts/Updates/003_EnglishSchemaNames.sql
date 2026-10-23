USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL
    THROW 50001, 'El esquema ya utiliza nombres en inglés.', 1;

IF OBJECT_ID(N'dbo.rol', N'U') IS NULL
    THROW 50002, 'Falta la tabla dbo.rol. Revisar la base seleccionada.', 1;
IF OBJECT_ID(N'dbo.permiso', N'U') IS NULL
    THROW 50002, 'Falta la tabla dbo.permiso. Revisar la base seleccionada.', 1;
IF OBJECT_ID(N'dbo.rol_permiso', N'U') IS NULL
    THROW 50002, 'Falta la tabla dbo.rol_permiso. Revisar la base seleccionada.', 1;
IF OBJECT_ID(N'dbo.usuario', N'U') IS NULL
    THROW 50002, 'Falta la tabla dbo.usuario. Revisar la base seleccionada.', 1;
IF OBJECT_ID(N'dbo.categoria', N'U') IS NULL
    THROW 50002, 'Falta la tabla dbo.categoria. Revisar la base seleccionada.', 1;

IF EXISTS (SELECT LTRIM(RTRIM(nombre)) FROM dbo.categoria GROUP BY LTRIM(RTRIM(nombre)) HAVING COUNT(*) > 1)
    THROW 50003, 'Hay categorías con nombres repetidos. Resolverlos antes de continuar.', 1;

IF EXISTS (SELECT 1 FROM dbo.categoria WHERE LEN(LTRIM(RTRIM(nombre))) = 0)
    THROW 50004, 'Hay categorías sin nombre. Corregirlas antes de continuar.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    EXEC sys.sp_rename N'dbo.rol.id_rol', N'Id', N'COLUMN';
    EXEC sys.sp_rename N'dbo.rol.nombre', N'Name', N'COLUMN';
    EXEC sys.sp_rename N'dbo.rol.es_personalizado', N'IsCustom', N'COLUMN';
    EXEC sys.sp_rename N'dbo.rol', N'Roles';

    EXEC sys.sp_rename N'dbo.permiso.id_permiso', N'Id', N'COLUMN';
    EXEC sys.sp_rename N'dbo.permiso.nombre', N'Name', N'COLUMN';
    EXEC sys.sp_rename N'dbo.permiso.descripcion', N'Description', N'COLUMN';
    EXEC sys.sp_rename N'dbo.permiso', N'Permissions';

    EXEC sys.sp_rename N'dbo.rol_permiso.id_rol', N'RoleId', N'COLUMN';
    EXEC sys.sp_rename N'dbo.rol_permiso.id_permiso', N'PermissionId', N'COLUMN';
    EXEC sys.sp_rename N'dbo.rol_permiso', N'RolePermissions';

    EXEC sys.sp_rename N'dbo.usuario.id_usuario', N'Id', N'COLUMN';
    EXEC sys.sp_rename N'dbo.usuario.id_rol', N'RoleId', N'COLUMN';
    EXEC sys.sp_rename N'dbo.usuario.nombre_usuario', N'UserName', N'COLUMN';
    EXEC sys.sp_rename N'dbo.usuario.password_hash', N'PasswordHash', N'COLUMN';
    EXEC sys.sp_rename N'dbo.usuario.mail', N'Email', N'COLUMN';
    EXEC sys.sp_rename N'dbo.usuario.fecha_creacion', N'CreatedAtUtc', N'COLUMN';
    EXEC sys.sp_rename N'dbo.usuario', N'Users';

    EXEC sys.sp_rename N'dbo.categoria.id_categoria', N'Id', N'COLUMN';
    EXEC sys.sp_rename N'dbo.categoria.nombre', N'Name', N'COLUMN';
    EXEC sys.sp_rename N'dbo.categoria.descripcion', N'Description', N'COLUMN';
    EXEC sys.sp_rename N'dbo.categoria', N'Categories';

    IF OBJECT_ID(N'dbo.PK_rol') IS NOT NULL
        EXEC sys.sp_rename N'dbo.PK_rol', N'PK_Roles', N'OBJECT';
    IF OBJECT_ID(N'dbo.UQ_rol_nombre') IS NOT NULL
        EXEC sys.sp_rename N'dbo.UQ_rol_nombre', N'UQ_Roles_Name', N'OBJECT';
    IF OBJECT_ID(N'dbo.DF_rol_es_personalizado') IS NOT NULL
        EXEC sys.sp_rename N'dbo.DF_rol_es_personalizado', N'DF_Roles_IsCustom', N'OBJECT';
    IF OBJECT_ID(N'dbo.PK_permiso') IS NOT NULL
        EXEC sys.sp_rename N'dbo.PK_permiso', N'PK_Permissions', N'OBJECT';
    IF OBJECT_ID(N'dbo.UQ_permiso_nombre') IS NOT NULL
        EXEC sys.sp_rename N'dbo.UQ_permiso_nombre', N'UQ_Permissions_Name', N'OBJECT';
    IF OBJECT_ID(N'dbo.PK_rol_permiso') IS NOT NULL
        EXEC sys.sp_rename N'dbo.PK_rol_permiso', N'PK_RolePermissions', N'OBJECT';
    IF OBJECT_ID(N'dbo.FK_rol_permiso_rol') IS NOT NULL
        EXEC sys.sp_rename N'dbo.FK_rol_permiso_rol', N'FK_RolePermissions_Roles', N'OBJECT';
    IF OBJECT_ID(N'dbo.FK_rol_permiso_permiso') IS NOT NULL
        EXEC sys.sp_rename N'dbo.FK_rol_permiso_permiso', N'FK_RolePermissions_Permissions', N'OBJECT';
    IF OBJECT_ID(N'dbo.PK_usuario') IS NOT NULL
        EXEC sys.sp_rename N'dbo.PK_usuario', N'PK_Users', N'OBJECT';
    IF OBJECT_ID(N'dbo.UQ_usuario_nombre') IS NOT NULL
        EXEC sys.sp_rename N'dbo.UQ_usuario_nombre', N'UQ_Users_UserName', N'OBJECT';
    IF OBJECT_ID(N'dbo.DF_usuario_fecha_creacion') IS NOT NULL
        EXEC sys.sp_rename N'dbo.DF_usuario_fecha_creacion', N'DF_Users_CreatedAtUtc', N'OBJECT';
    IF OBJECT_ID(N'dbo.FK_usuario_rol') IS NOT NULL
        EXEC sys.sp_rename N'dbo.FK_usuario_rol', N'FK_Users_Roles', N'OBJECT';
    IF OBJECT_ID(N'dbo.PK_categoria') IS NOT NULL
        EXEC sys.sp_rename N'dbo.PK_categoria', N'PK_Categories', N'OBJECT';

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = N'IX_usuario_id_rol')
        EXEC sys.sp_rename N'dbo.Users.IX_usuario_id_rol', N'IX_Users_RoleId', N'INDEX';

    EXEC sys.sp_executesql N'
        UPDATE dbo.Categories SET Name = LTRIM(RTRIM(Name));
        ALTER TABLE dbo.Categories ADD CONSTRAINT UQ_Categories_Name UNIQUE (Name);
        ALTER TABLE dbo.Categories ADD CONSTRAINT CK_Categories_Name CHECK (LEN(LTRIM(RTRIM(Name))) > 0);';

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
