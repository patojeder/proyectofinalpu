# VENTA-STOCK

Base del proyecto final: ASP.NET Core MVC en .NET 10 y SQL Server, con Entity Framework Core Database First.

## Proyectos

- **VentaStock.Domain**: entidades, reglas de negocio y contratos de repositorios (`Contracts/`).
- **VentaStock.Application**: interfaces de servicios (`Contracts/`) e implementaciones (`Services/`).
- **VentaStock.Infrastructure**: contexto generado desde SQL Server y repositorios EF Core.
- **VentaStock.Web**: presentación MVC y registro de dependencias.

La aplicación incluye autenticación por cookies, administración de usuarios, categorías, servicios y un panel de módulos con accesos según permisos. Las constantes de roles y permisos se centralizan en `VentaStock.Domain/Security`.

## Ejecución

Requiere SDK .NET 10 y runtime ASP.NET Core 10.

```bash
dotnet restore VentaStock.sln
dotnet build VentaStock.sln
dotnet run --project src/VentaStock.Web --launch-profile https
```

Configurar la conexión con User Secrets o variables de entorno. Los scripts y su orden de ejecución están documentados en [Data/Scripts](src/VentaStock.Infrastructure/Data/Scripts/README.md). El modelo sigue el enfoque Database First: aplicar primero los cambios SQL y después regenerar entidades y DbContext.

Las tablas y columnas usan nombres en inglés. Usuarios, categorías y servicios usan borrado físico; categorías y servicios tienen nombres únicos. Para una base anterior, aplicar los scripts de actualización antes de ejecutar esta versión de la aplicación.

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
