-- 1. Crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'VentaStockDb')
BEGIN
    CREATE DATABASE VentaStockDb;
END
GO

USE VentaStockDb;
GO

-- 2. Eliminar tablas previas si existen (en orden de dependencia)
IF OBJECT_ID('dbo.refresh_token', 'U') IS NOT NULL DROP TABLE dbo.refresh_token;
IF OBJECT_ID('dbo.rol_permiso', 'U') IS NOT NULL DROP TABLE dbo.rol_permiso;
IF OBJECT_ID('dbo.permiso', 'U') IS NOT NULL DROP TABLE dbo.permiso;
IF OBJECT_ID('dbo.usuario', 'U') IS NOT NULL DROP TABLE dbo.usuario;
IF OBJECT_ID('dbo.rol', 'U') IS NOT NULL DROP TABLE dbo.rol;
GO

-- 3. Tabla: rol
CREATE TABLE dbo.rol (
    id_rol INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(50) NOT NULL,
    es_personalizado BIT NOT NULL DEFAULT 0
);
GO

-- 4. Tabla: permiso
CREATE TABLE dbo.permiso (
    id_permiso INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(255) NULL
);
GO

-- 5. Tabla: rol_permiso (Intermedia N:M)
CREATE TABLE dbo.rol_permiso (
    id_rol INT NOT NULL,
    id_permiso INT NOT NULL,
    CONSTRAINT PK_rol_permiso PRIMARY KEY (id_rol, id_permiso),
    CONSTRAINT FK_rol_permiso_rol FOREIGN KEY (id_rol) REFERENCES dbo.rol(id_rol) ON DELETE CASCADE,
    CONSTRAINT FK_rol_permiso_permiso FOREIGN KEY (id_permiso) REFERENCES dbo.permiso(id_permiso) ON DELETE CASCADE
);
GO

-- 6. Tabla: usuario
CREATE TABLE dbo.usuario (
    id_usuario INT IDENTITY(1,1) PRIMARY KEY,
    id_rol INT NOT NULL,
    nombre_usuario NVARCHAR(50) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    mail NVARCHAR(100) NOT NULL,
    activo BIT NOT NULL DEFAULT 1,
    fecha_creacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_usuario_rol FOREIGN KEY (id_rol) REFERENCES dbo.rol(id_rol)
);
GO

-- 7. Tabla: refresh_token
CREATE TABLE dbo.refresh_token (
    id_refresh_token INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    token NVARCHAR(255) NOT NULL,
    fecha_expiracion DATETIME2 NOT NULL,
    revocado BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_refresh_token_usuario FOREIGN KEY (id_usuario) REFERENCES dbo.usuario(id_usuario) ON DELETE CASCADE
);
GO

-- 8. Datos iniciales de Roles y Permisos
INSERT INTO dbo.rol (nombre, es_personalizado) 
VALUES ('Administrador', 0), ('Vendedor', 0);

INSERT INTO dbo.permiso (nombre, descripcion) 
VALUES 
('VENTAS_CREAR', 'Permite registrar nuevas ventas'),
('VENTAS_VER', 'Permite consultar el listado de ventas'),
('STOCK_ADMIN', 'Permite gestionar stock y productos'),
('USUARIOS_ADMIN', 'Permite administrar cuentas de usuarios');

-- Asignar permisos al Administrador (id_rol = 1)
INSERT INTO dbo.rol_permiso (id_rol, id_permiso)
SELECT 1, id_permiso FROM dbo.permiso;

-- 9. Insertar usuario de prueba (contraseña: prueba123)
-- Hash BCrypt para 'prueba123': $2a$11$qN15wZg6n4qEwVfK03J5Uugz.Xvj2T6zNqE90T0K1Xp2mN9.XGf9y
INSERT INTO dbo.usuario (id_rol, nombre_usuario, password_hash, mail, activo, fecha_creacion)
VALUES (
    1, 
    'AugustoPrueba', 
    '$2a$11$qN15wZg6n4qEwVfK03J5Uugz.Xvj2T6zNqE90T0K1Xp2mN9.XGf9y', 
    'augusto@puntocolor.com', 
    1, 
    SYSDATETIME()
);
GO