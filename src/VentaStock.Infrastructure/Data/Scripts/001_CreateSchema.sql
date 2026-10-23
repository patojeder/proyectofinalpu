USE [VentaStockDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.rol') IS NOT NULL
    OR OBJECT_ID(N'dbo.permiso') IS NOT NULL
    OR OBJECT_ID(N'dbo.rol_permiso') IS NOT NULL
    OR OBJECT_ID(N'dbo.usuario') IS NOT NULL
    OR OBJECT_ID(N'dbo.categoria') IS NOT NULL
    THROW 50001, 'Ya existen tablas de la aplicación. Usar un script de actualización.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.rol (
        id_rol INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_rol PRIMARY KEY,
        nombre NVARCHAR(50) NOT NULL CONSTRAINT UQ_rol_nombre UNIQUE,
        es_personalizado BIT NOT NULL CONSTRAINT DF_rol_es_personalizado DEFAULT (0)
    );

    CREATE TABLE dbo.permiso (
        id_permiso INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_permiso PRIMARY KEY,
        nombre NVARCHAR(100) NOT NULL CONSTRAINT UQ_permiso_nombre UNIQUE,
        descripcion NVARCHAR(255) NULL
    );

    CREATE TABLE dbo.rol_permiso (
        id_rol INT NOT NULL,
        id_permiso INT NOT NULL,
        CONSTRAINT PK_rol_permiso PRIMARY KEY (id_rol, id_permiso),
        CONSTRAINT FK_rol_permiso_rol FOREIGN KEY (id_rol) REFERENCES dbo.rol(id_rol) ON DELETE CASCADE,
        CONSTRAINT FK_rol_permiso_permiso FOREIGN KEY (id_permiso) REFERENCES dbo.permiso(id_permiso) ON DELETE CASCADE
    );

    CREATE TABLE dbo.usuario (
        id_usuario INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_usuario PRIMARY KEY,
        id_rol INT NOT NULL,
        nombre_usuario NVARCHAR(50) NOT NULL CONSTRAINT UQ_usuario_nombre UNIQUE,
        password_hash NVARCHAR(255) NOT NULL,
        mail NVARCHAR(100) NOT NULL,
        fecha_creacion DATETIME2 NOT NULL CONSTRAINT DF_usuario_fecha_creacion DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_usuario_rol FOREIGN KEY (id_rol) REFERENCES dbo.rol(id_rol)
    );

    CREATE INDEX IX_usuario_id_rol ON dbo.usuario(id_rol);

    CREATE TABLE dbo.categoria (
        id_categoria INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_categoria PRIMARY KEY,
        nombre NVARCHAR(100) NOT NULL,
        descripcion NVARCHAR(MAX) NULL
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
