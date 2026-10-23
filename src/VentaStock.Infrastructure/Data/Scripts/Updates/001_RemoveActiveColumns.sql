-- Solo para bases existentes que todavía tengan las columnas activo.
-- Se conservan todas las filas, incluidas las que estaban inactivas.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 50001, 'Seleccioná la base de la aplicación antes de ejecutar este script.', 1;

IF OBJECT_ID(N'dbo.usuario', N'U') IS NULL OR OBJECT_ID(N'dbo.categoria', N'U') IS NULL
    THROW 50002, 'Deben existir las tablas dbo.usuario y dbo.categoria.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Sql NVARCHAR(MAX) = N'';

    SELECT @Sql = @Sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(d.parent_object_id))
        + N'.' + QUOTENAME(OBJECT_NAME(d.parent_object_id))
        + N' DROP CONSTRAINT ' + QUOTENAME(d.name) + N';'
    FROM sys.default_constraints AS d
    INNER JOIN sys.columns AS c
        ON c.object_id = d.parent_object_id AND c.column_id = d.parent_column_id
    WHERE d.parent_object_id IN (OBJECT_ID(N'dbo.usuario'), OBJECT_ID(N'dbo.categoria'))
        AND c.name = N'activo';

    IF LEN(@Sql) > 0
        EXEC sys.sp_executesql @Sql;

    IF COL_LENGTH(N'dbo.usuario', N'activo') IS NOT NULL
        ALTER TABLE dbo.usuario DROP COLUMN activo;

    IF COL_LENGTH(N'dbo.categoria', N'activo') IS NOT NULL
        ALTER TABLE dbo.categoria DROP COLUMN activo;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
