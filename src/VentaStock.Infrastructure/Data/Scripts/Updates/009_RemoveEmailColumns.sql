USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 50001, 'Seleccioná la base de la aplicación antes de ejecutar este script.', 1;

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL OR OBJECT_ID(N'dbo.Customers', N'U') IS NULL
    THROW 50002, 'Deben existir las tablas dbo.Users y dbo.Customers. Ejecutar primero las actualizaciones anteriores.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @TableName SYSNAME;
    DECLARE @ObjectId INT;
    DECLARE @EmailColumnId INT;
    DECLARE @Sql NVARCHAR(MAX);

    DECLARE EmailTables CURSOR LOCAL FAST_FORWARD FOR
        SELECT TableName FROM (VALUES (N'Users'), (N'Customers')) AS TargetTables(TableName);

    OPEN EmailTables;
    FETCH NEXT FROM EmailTables INTO @TableName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ObjectId = OBJECT_ID(N'dbo.' + QUOTENAME(@TableName), N'U');
        SET @EmailColumnId = NULL;

        SELECT @EmailColumnId = column_id
        FROM sys.columns
        WHERE object_id = @ObjectId AND name = N'Email';

        IF @EmailColumnId IS NOT NULL
        BEGIN
            SELECT @Sql = STRING_AGG(
                N'ALTER TABLE dbo.' + QUOTENAME(@TableName) + N' DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';',
                N' ')
            FROM sys.default_constraints AS dc
            WHERE dc.parent_object_id = @ObjectId AND dc.parent_column_id = @EmailColumnId;
            IF @Sql IS NOT NULL EXEC sys.sp_executesql @Sql;

            SELECT @Sql = STRING_AGG(
                N'ALTER TABLE dbo.' + QUOTENAME(@TableName) + N' DROP CONSTRAINT ' + QUOTENAME(cc.name) + N';',
                N' ')
            FROM sys.check_constraints AS cc
            WHERE cc.parent_object_id = @ObjectId AND cc.definition LIKE N'%Email%';
            IF @Sql IS NOT NULL EXEC sys.sp_executesql @Sql;

            SELECT @Sql = STRING_AGG(
                N'ALTER TABLE dbo.' + QUOTENAME(@TableName) + N' DROP CONSTRAINT ' + QUOTENAME(kc.name) + N';',
                N' ')
            FROM sys.key_constraints AS kc
            WHERE kc.parent_object_id = @ObjectId AND kc.type = N'UQ'
                AND EXISTS (
                    SELECT 1
                    FROM sys.index_columns AS ic
                    INNER JOIN sys.columns AS c
                        ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                    WHERE ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
                        AND c.column_id = @EmailColumnId
                );
            IF @Sql IS NOT NULL EXEC sys.sp_executesql @Sql;

            SELECT @Sql = STRING_AGG(
                N'DROP INDEX ' + QUOTENAME(i.name) + N' ON dbo.' + QUOTENAME(@TableName) + N';',
                N' ')
            FROM sys.indexes AS i
            WHERE i.object_id = @ObjectId AND i.index_id > 0
                AND i.is_primary_key = 0 AND i.is_unique_constraint = 0
                AND EXISTS (
                    SELECT 1
                    FROM sys.index_columns AS ic
                    WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id
                        AND ic.column_id = @EmailColumnId
                );
            IF @Sql IS NOT NULL EXEC sys.sp_executesql @Sql;

            SET @Sql = N'ALTER TABLE dbo.' + QUOTENAME(@TableName) + N' DROP COLUMN [Email];';
            EXEC sys.sp_executesql @Sql;
        END;

        FETCH NEXT FROM EmailTables INTO @TableName;
    END;

    CLOSE EmailTables;
    DEALLOCATE EmailTables;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local', 'EmailTables') >= 0 CLOSE EmailTables;
    IF CURSOR_STATUS('local', 'EmailTables') > -3 DEALLOCATE EmailTables;
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
