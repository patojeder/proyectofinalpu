# Scripts de base de datos

## Base nueva

Crear `VentaStockDb` y ejecutar en orden:

1. `001_CreateSchema.sql`: crea `Users`, `Roles`, `Permissions`, `RolePermissions`, `Categories`, `Services`, `ServiceCategories`, `Products`, `ProductCategories`, `ProductStocks` y `StockMovements`. Requiere una base vacía y no elimina tablas ni datos.
2. `002_SeedRolesAndPermissions.sql`: agrega los roles y permisos iniciales sin duplicar los existentes. No crea usuarios ni categorías de prueba.

Ambos scripts seleccionan explícitamente `VentaStockDb`. Las tablas y columnas usan nombres en inglés. No hay columna de baja lógica ni tabla de refresh tokens: el login utiliza cookies de ASP.NET Core.

| Rol | Permisos iniciales |
|---|---|
| Dueño | Todos los permisos definidos en el script |
| Gestor de Usuarios | `Users.Read`, `Users.Create`, `Users.Update`, `Users.Delete`, `Users.ResetPassword` |
| Gestor de Productos | Administración de categorías, servicios y productos; consulta/edición de precios y `Stock.Read` |
| Gestor de Stock | `Products.Read`, `Products.Create`, `Stock.Read`, `Stock.Receipt`, `Stock.Adjust`, `Stock.ReadMovements`, `Categories.Read` |
| Gestor de Precios | `Products.Read`, `Products.ReadCost`, `Products.ReadSalePrice`, `Products.UpdatePrices`, `Categories.Read` |
| Vendedor | `Sales.Read`, `Sales.Create`, `Categories.Read`, `Services.Read`, `Products.Read`, `Products.ReadSalePrice`, `Stock.Read` |

Son los roles necesarios para las funciones actuales. Los demás roles predefinidos del proyecto se incorporarán al implementar sus módulos.

Las tablas, columnas, clases y constantes usan nombres en inglés. Los nombres almacenados de los roles están en español y se muestran directamente en la interfaz.

Los identificadores de permisos usan `Modulo.Operacion` en inglés y PascalCase. Deben coincidir con las constantes de `VentaStock.Domain/Security/Permissions`. Las descripciones visibles están en español. Los roles personalizados reciben permisos explícitamente.

Categorías, servicios y productos no admiten nombres repetidos: se valida en Application y mediante restricciones únicas en SQL Server. Los nombres se recortan antes de guardar. La comparación sigue la collation de la base; el entorno local usa comparación sin distinguir mayúsculas.

Un servicio tiene nombre, descripción y categorías opcionales, sin precio fijo ni stock. Al eliminarlo se retiran sus asociaciones; una categoría utilizada por servicios o productos no se puede eliminar. Los permisos de alta o edición de servicios incluyen consultar nombres de categorías para completar el formulario, sin conceder su administración.

`Products` guarda nombre, descripción, código de barras, sector, estante y precios opcionales. El nombre y el código son únicos. Si se omite el código, se genera `INT-` más un identificador único. `ProductCategories` asocia las categorías; la aplicación exige al menos una. `ProductStocks` contiene una fila por producto, con cantidad y mínimo iniciales en 0. El mínimo no puede ser negativo; la cantidad sí admite negativos. El ABM no modifica la cantidad disponible.

Los precios deben estar ambos vacíos o ambos asignados, con costo no negativo y venta mayor al costo. El Gestor de Stock crea productos con ambos valores vacíos y no los consulta. Para editarlos se requieren `Products.UpdatePrices`, `Products.ReadCost` y `Products.ReadSalePrice`.

`StockMovements` registra producto, usuario, nombre del usuario en ese momento, tipo (`Receipt`, `Adjustment` u `OpeningBalance`), variación, saldo anterior/posterior, motivo, fecha UTC e identificador único de operación. `QuantityChange` es BIGINT para admitir diferencias entre los extremos de un saldo INT. Los ingresos son positivos; los ajustes exigen motivo y cantidad final no negativa. El saldo disponible puede ser negativo.

Saldo y movimiento se guardan en una sola transacción con bloqueo de actualización de la fila de `ProductStocks`. `OperationId` impide duplicar el mismo formulario. Los ajustes comprueban además el saldo que se mostraba al iniciar el conteo. El código no ofrece edición ni eliminación de movimientos. La clave foránea impide eliminar un producto con historial; si se elimina la cuenta del autor, `UserId` queda NULL y `UserName` se conserva.

Todavía no hay tablas de ventas. Al incorporarlas, sus claves foráneas también deberán preservar el historial y el descuento de stock deberá integrarse en la transacción de venta.

La aplicación reserva la gestión de cuentas Dueño y la asignación de ese rol al propio Dueño. No se concede ese privilegio mediante permisos de usuarios.

La primera cuenta Dueño se creará por separado con una contraseña hasheada mediante BCrypt. No guardar contraseñas reales ni credenciales iniciales fijas en estos scripts.

## Base existente

No ejecutar el script de creación sobre tablas existentes. Seleccionar la base correcta y aplicar según corresponda:

1. `Updates/001_RemoveActiveColumns.sql`: elimina `activo` y sus restricciones DEFAULT en usuarios/categorías. Conserva todas las filas; los registros antes inactivos vuelven a estar disponibles.
2. `Updates/002_NormalizePermissions.sql`: convierte los permisos antiguos a los nuevos conservando las asignaciones. Retira `USUARIOS_ADMIN` y `STOCK_ADMIN`, que el código ya no utiliza, sin convertirlos en permisos granulares automáticamente. También renombra Administrador a Owner si no existe otro Owner.
3. `Updates/003_EnglishSchemaNames.sql`: renombra las cinco tablas existentes y sus columnas, conservando IDs, usuarios, hashes y asignaciones. Se detiene si hay nombres de categorías duplicados o vacíos antes de agregar la restricción única.
4. `Updates/004_CreateServices.sql`: crea servicios y su relación con categorías en el esquema en inglés.
5. `Updates/005_SpanishRoleNames.sql`: cambia los nombres de los roles a español, conservando IDs y asignaciones.
6. `Updates/006_CreateProducts.sql`: crea productos, sus categorías y su stock; se detiene si ya existen esas tablas.
7. `Updates/007_CreateStockMovements.sql`: crea el historial; conserva saldos existentes distintos de 0 como movimientos `OpeningBalance` del actor Sistema. Se detiene si la tabla ya existe.
8. `002_SeedRolesAndPermissions.sql`: incorpora los permisos y asignaciones base que falten, incluidos ingresos, ajustes e historial de stock.

Para una base en inglés que ya tiene productos y stock, aplicar únicamente el paso 7 y después el 8. Si todavía no tiene productos, aplicar antes el paso 6. No repetir los pasos 6 o 7 si sus tablas ya existen. Las bases nuevas creadas con el esquema actual ya incluyen estas tablas y solo necesitan el seed.

Los pasos 1 y 2 se aplican solo si la base todavía tiene columnas o permisos antiguos. Los pasos 3 y 4 son para la base anterior en español; no ejecutarlos sobre una inicialización nueva con `001_CreateSchema.sql`. El paso 5 se aplica a bases que tengan los nombres de roles anteriores en inglés.

Cerrar sesión y volver a ingresar después de normalizar permisos o renombrar roles, porque las cookies existentes conservan sus claims anteriores.

## Database First

Después de aplicar el esquema, regenerar entidades y DbContext con EF Core y adaptar las referencias a las columnas retiradas. Cambiar los archivos SQL no modifica una base existente ni actualiza por sí solo el modelo de C#.

Las entidades están en `VentaStock.Domain/Entities` y el contexto en `VentaStock.Infrastructure/Data`. Usar `--no-onconfiguring` y una conexión por configuración para no incluir credenciales en el código generado. El esquema en inglés genera las entidades `User`, `Role`, `Permission`, `Category`, `Service`, `Product`, `ProductStock` y `StockMovement`; las relaciones de roles/permisos, servicios/categorías y productos/categorías se representan mediante colecciones.

Al regenerar después de renombrar tablas, eliminar los archivos de entidades anteriores en español para no mantener modelos obsoletos. Antes de sobrescribir el modelo, conservar una copia o tener sus cambios confirmados en Git.

Los scripts anteriores se archivaron fuera del repositorio, en `referencias/respaldo/sql-anterior-*`.
