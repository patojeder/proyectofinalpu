# VENTA-STOCK

Base del proyecto final: ASP.NET Core MVC en .NET 10 y SQL Server, con Entity Framework Core Database First.

## Proyectos

- **VentaStock.Domain**: entidades, reglas de negocio y contratos de repositorios (`Contracts/`).
- **VentaStock.Application**: interfaces de servicios (`Contracts/`) e implementaciones (`Services/`).
- **VentaStock.Infrastructure**: contexto generado desde SQL Server y repositorios EF Core.
- **VentaStock.Web**: presentación MVC y registro de dependencias.

La aplicación incluye autenticación por cookies, administración de usuarios, categorías, servicios, productos y un panel de módulos con accesos según permisos. Las constantes de roles y permisos se centralizan en `VentaStock.Domain/Security`.

## Ejecución

Requiere SDK .NET 10 y runtime ASP.NET Core 10.

```bash
dotnet restore VentaStock.sln
dotnet build VentaStock.sln
dotnet run --project src/VentaStock.Web --launch-profile https
```

Configurar la conexión con User Secrets o variables de entorno. Los scripts y su orden de ejecución están documentados en [Data/Scripts](src/VentaStock.Infrastructure/Data/Scripts/README.md). El modelo sigue el enfoque Database First: aplicar primero los cambios SQL y después regenerar entidades y DbContext.

Las tablas y columnas usan nombres en inglés. Usuarios, categorías, servicios y productos usan borrado físico; categorías, servicios y productos tienen nombres únicos. Para una base anterior, aplicar los scripts de actualización antes de ejecutar esta versión de la aplicación.

## Productos

El módulo incluye listado, alta, modificación de datos generales, edición separada de precios y eliminación. Los botones y los datos de precios/stock se muestran según permisos; el controlador y el servicio también verifican las autorizaciones.

- Código de barras opcional al crear: si se omite, se genera un código interno único `INT-…`. Al editar, dejarlo vacío conserva el código actual.
- Una o varias categorías obligatorias; descripción, sector y estante opcionales.
- Stock mínimo entero no negativo, con valor 0 si se omite. Las existencias iniciales son 0 y no se editan desde este ABM.
- Costo y precio de venta pueden quedar ambos sin asignar. Si se cargan, el costo debe ser no negativo y la venta mayor que el costo, con hasta dos decimales.
- El Gestor de Stock puede crear sin acceder a precios. El Gestor de Productos y el Gestor de Precios pueden asignarlos. El Vendedor solo consulta el precio de venta y las existencias.
- Se impide eliminar un producto con existencias positivas o negativas. Los futuros movimientos y ventas deberán conservar su historial mediante relaciones que impidan su eliminación.

Los movimientos de stock se implementarán en otra tarea; la estructura admite cantidades negativas y unidades enteras. Después de actualizar los permisos, cerrar sesión y volver a ingresar para renovar los claims.

## Configuración y secretos

- Desarrollo: User Secrets, almacenados fuera del repositorio. Cada integrante configura sus propios valores.
- Despliegue: variables de entorno del proceso o servicio. `ConnectionStrings__VentaStock` corresponde a `ConnectionStrings:VentaStock`; `Jwt__SigningKey` a `Jwt:SigningKey`.
- `.env.example` contiene únicamente marcadores; `.env` está ignorado. ASP.NET Core no carga archivos `.env` automáticamente.
- Las variables de entorno tienen prioridad sobre User Secrets y appsettings. Los argumentos de línea de comandos tienen prioridad aún mayor.

Ejemplo ilustrativo (reemplazar el marcador; no contiene una conexión real):

```bash
dotnet user-secrets set "ConnectionStrings:VentaStock" "CADENA_LOCAL" --project src/VentaStock.Web
```

User Secrets se carga automáticamente en Development y no cifra los valores. No poner secretos en appsettings, launchSettings ni en el repositorio. La aplicación consulta SQL Server y utiliza autenticación por cookies; las variables JWT están reservadas para una posible API futura.

## Inyección de dependencias

`Program.cs` llama a `AddApplication()` y `AddInfrastructure(builder.Configuration)`. Cada capa centraliza sus registros en `DependencyInjection.cs`, siguiendo el patrón de TSTCo.

Los servicios y repositorios se registran con `AddScoped<Interfaz, Implementacion>()` y el contexto con `AddDbContext`. `ILogger<T>` lo proporciona el host ASP.NET Core; no necesita un registro manual.
