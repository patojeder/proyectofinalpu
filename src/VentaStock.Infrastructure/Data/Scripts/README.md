# Scripts de base de datos

## Base nueva

Crear `VentaStockDb` y ejecutar en orden:

1. `001_CreateSchema.sql`: crea las cinco tablas actuales. Se detiene si ya existe alguna; no elimina tablas ni datos.
2. `002_SeedRolesAndPermissions.sql`: agrega los roles y permisos iniciales sin duplicar los existentes. No crea usuarios ni categorías de prueba.

Ambos scripts seleccionan explícitamente `VentaStockDb`. No hay columna `activo` ni tabla de refresh tokens: el login utiliza cookies de ASP.NET Core.

| Rol | Permisos iniciales |
|---|---|
| Owner | Todos los permisos definidos en el script |
| UserManager | `Users.Read`, `Users.Create`, `Users.Update`, `Users.Delete`, `Users.ResetPassword` |
| ProductManager | `Categories.Read`, `Categories.Create`, `Categories.Update`, `Categories.Delete` |
| Vendedor | `Sales.Read`, `Sales.Create`, `Categories.Read` |

Son los roles necesarios para las funciones actuales. Los demás roles predefinidos del proyecto se incorporarán al implementar sus módulos.

Los identificadores de permisos usan `Modulo.Operacion` en inglés y PascalCase. Deben coincidir con las constantes de `VentaStock.Domain/Security/Permissions`. Las descripciones visibles están en español. Los roles personalizados reciben permisos explícitamente.

La aplicación reserva la gestión de cuentas Owner y la asignación de ese rol al propio Owner. No se concede ese privilegio mediante permisos de usuarios.

La primera cuenta Owner se creará por separado con una contraseña hasheada mediante BCrypt. No guardar contraseñas reales ni credenciales iniciales fijas en estos scripts.

## Base existente

No ejecutar el script de creación sobre tablas existentes. Seleccionar la base correcta y aplicar según corresponda:

1. `Updates/001_RemoveActiveColumns.sql`: elimina `activo` y sus restricciones DEFAULT en usuarios/categorías. Conserva todas las filas; los registros antes inactivos vuelven a estar disponibles.
2. `Updates/002_NormalizePermissions.sql`: convierte los permisos antiguos a los nuevos conservando las asignaciones. Retira `USUARIOS_ADMIN` y `STOCK_ADMIN`, que el código ya no utiliza, sin convertirlos en permisos granulares automáticamente. También renombra Administrador a Owner si no existe otro Owner.
3. `002_SeedRolesAndPermissions.sql`: incorpora los permisos y asignaciones base que falten.

Cerrar sesión y volver a ingresar después de normalizar permisos, porque las cookies existentes conservan sus claims anteriores.

## Database First

Después de aplicar el esquema, regenerar entidades y DbContext con EF Core y adaptar las referencias a las columnas retiradas. Cambiar los archivos SQL no modifica una base existente ni actualiza por sí solo el modelo de C#.

Las entidades están en `VentaStock.Domain/Entities` y el contexto en `VentaStock.Infrastructure/Data`. Usar `--no-onconfiguring` y una conexión por configuración para no incluir credenciales en el código generado. EF interpreta `categoria` como `Categorium`: después de regenerar, conservar el nombre `Categoria` de la entidad y `Categorias` del DbSet, retirando el archivo `Categorium.cs` generado.

Los scripts anteriores se archivaron fuera del repositorio, en `referencias/respaldo/sql-anterior-*`.
