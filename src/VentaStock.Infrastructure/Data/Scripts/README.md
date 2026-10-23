# Scripts de base de datos

## Base nueva

Crear `VentaStockDb` y ejecutar en orden:

1. `001_CreateSchema.sql`: crea `Users`, `Roles`, `Permissions`, `RolePermissions`, `Categories`, `Services` y `ServiceCategories`. Requiere una base vacía y no elimina tablas ni datos.
2. `002_SeedRolesAndPermissions.sql`: agrega los roles y permisos iniciales sin duplicar los existentes. No crea usuarios ni categorías de prueba.

Ambos scripts seleccionan explícitamente `VentaStockDb`. Las tablas y columnas usan nombres en inglés. No hay columna de baja lógica ni tabla de refresh tokens: el login utiliza cookies de ASP.NET Core.

| Rol | Permisos iniciales |
|---|---|
| Dueño | Todos los permisos definidos en el script |
| Gestor de Usuarios | `Users.Read`, `Users.Create`, `Users.Update`, `Users.Delete`, `Users.ResetPassword` |
| Gestor de Productos | `Categories.Read/Create/Update/Delete` y `Services.Read/Create/Update/Delete` |
| Vendedor | `Sales.Read`, `Sales.Create`, `Categories.Read`, `Services.Read` |

Son los roles necesarios para las funciones actuales. Los demás roles predefinidos del proyecto se incorporarán al implementar sus módulos.

Las tablas, columnas, clases y constantes usan nombres en inglés. Los nombres almacenados de los roles están en español y se muestran directamente en la interfaz.

Los identificadores de permisos usan `Modulo.Operacion` en inglés y PascalCase. Deben coincidir con las constantes de `VentaStock.Domain/Security/Permissions`. Las descripciones visibles están en español. Los roles personalizados reciben permisos explícitamente.

Categorías y servicios no admiten nombres repetidos: se valida en Application y mediante restricciones únicas en SQL Server. Los nombres se recortan antes de guardar. La comparación sigue la collation de la base; el entorno local usa comparación sin distinguir mayúsculas.

Un servicio tiene nombre, descripción y categorías opcionales, sin precio fijo ni stock. Al eliminarlo se retiran sus asociaciones; una categoría utilizada por servicios no se puede eliminar. Los permisos de alta o edición de servicios incluyen consultar nombres de categorías para completar el formulario, sin conceder su administración.

La aplicación reserva la gestión de cuentas Dueño y la asignación de ese rol al propio Dueño. No se concede ese privilegio mediante permisos de usuarios.

La primera cuenta Dueño se creará por separado con una contraseña hasheada mediante BCrypt. No guardar contraseñas reales ni credenciales iniciales fijas en estos scripts.

## Base existente

No ejecutar el script de creación sobre tablas existentes. Seleccionar la base correcta y aplicar según corresponda:

1. `Updates/001_RemoveActiveColumns.sql`: elimina `activo` y sus restricciones DEFAULT en usuarios/categorías. Conserva todas las filas; los registros antes inactivos vuelven a estar disponibles.
2. `Updates/002_NormalizePermissions.sql`: convierte los permisos antiguos a los nuevos conservando las asignaciones. Retira `USUARIOS_ADMIN` y `STOCK_ADMIN`, que el código ya no utiliza, sin convertirlos en permisos granulares automáticamente. También renombra Administrador a Owner si no existe otro Owner.
3. `Updates/003_EnglishSchemaNames.sql`: renombra las cinco tablas existentes y sus columnas, conservando IDs, usuarios, hashes y asignaciones. Se detiene si hay nombres de categorías duplicados o vacíos antes de agregar la restricción única.
4. `Updates/004_CreateServices.sql`: crea servicios y su relación con categorías en el esquema en inglés.
5. `Updates/005_SpanishRoleNames.sql`: cambia los nombres de los roles a español, conservando IDs y asignaciones.
6. `002_SeedRolesAndPermissions.sql`: incorpora los permisos y asignaciones base que falten, incluidos los de servicios.

Los pasos 1 y 2 se aplican solo si la base todavía tiene columnas o permisos antiguos. Los pasos 3 y 4 son para la base anterior en español; no ejecutarlos sobre una inicialización nueva con `001_CreateSchema.sql`. El paso 5 se aplica a bases que tengan los nombres de roles anteriores en inglés.

Cerrar sesión y volver a ingresar después de normalizar permisos o renombrar roles, porque las cookies existentes conservan sus claims anteriores.

## Database First

Después de aplicar el esquema, regenerar entidades y DbContext con EF Core y adaptar las referencias a las columnas retiradas. Cambiar los archivos SQL no modifica una base existente ni actualiza por sí solo el modelo de C#.

Las entidades están en `VentaStock.Domain/Entities` y el contexto en `VentaStock.Infrastructure/Data`. Usar `--no-onconfiguring` y una conexión por configuración para no incluir credenciales en el código generado. El esquema en inglés genera las entidades `User`, `Role`, `Permission`, `Category` y `Service`; las relaciones de roles/permisos y servicios/categorías se representan mediante colecciones.

Al regenerar después de renombrar tablas, eliminar los archivos de entidades anteriores en español para no mantener modelos obsoletos. Antes de sobrescribir el modelo, conservar una copia o tener sus cambios confirmados en Git.

Los scripts anteriores se archivaron fuera del repositorio, en `referencias/respaldo/sql-anterior-*`.
