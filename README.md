# VENTA-STOCK

Base del proyecto final: ASP.NET Core MVC en .NET 10 y SQL Server, con Entity Framework Core Database First.

## Proyectos

- **VentaStock.Domain**: entidades, reglas de negocio y contratos de repositorios (`Contracts/`).
- **VentaStock.Application**: interfaces de servicios (`Contracts/`) e implementaciones (`Services/`).
- **VentaStock.Infrastructure**: contexto generado desde SQL Server y repositorios EF Core.
- **VentaStock.Web**: presentación MVC y registro de dependencias.

Las bibliotecas tienen únicamente sus puntos de registro de DI, sin lógica de negocio, y Web conserva la plantilla MVC estándar. No hay conexión a base de datos ni funcionalidades de negocio implementadas.

## Ejecución

Requiere SDK .NET 10 y runtime ASP.NET Core 10.

```bash
dotnet restore VentaStock.sln
dotnet build VentaStock.sln
dotnet run --project src/VentaStock.Web --launch-profile https
```

El esquema SQL se definirá antes de generar entidades y DbContext. El proveedor SQL Server y EF Core Design están referenciados en sus proyectos. Registrar servicios y repositorios en DI cuando se implementen; configurar la conexión con user-secrets o variables de entorno.

## Configuración y secretos

- Desarrollo: User Secrets, almacenados fuera del repositorio. Cada integrante configura sus propios valores.
- Despliegue: variables de entorno del proceso o servicio. `ConnectionStrings__VentaStock` corresponde a `ConnectionStrings:VentaStock`; `Jwt__SigningKey` a `Jwt:SigningKey`.
- `.env.example` contiene únicamente marcadores; `.env` está ignorado. ASP.NET Core no carga archivos `.env` automáticamente.
- Las variables de entorno tienen prioridad sobre User Secrets y appsettings. Los argumentos de línea de comandos tienen prioridad aún mayor.

Ejemplo ilustrativo (reemplazar el marcador; no contiene una conexión real):

```bash
dotnet user-secrets set "ConnectionStrings:VentaStock" "CADENA_LOCAL" --project src/VentaStock.Web
```

User Secrets se carga automáticamente en Development y no cifra los valores. No poner secretos en appsettings, launchSettings ni en el repositorio. La base todavía no consulta SQL Server ni utiliza JWT; estas claves son convenciones para sus futuras implementaciones.

## Inyección de dependencias

`Program.cs` llama a `AddApplication()` y `AddInfrastructure(builder.Configuration)`. Cada capa centraliza sus registros en `DependencyInjection.cs`, siguiendo el patrón de TSTCo.

Agregar servicios y repositorios con `AddScoped<Interfaz, Implementacion>()` cuando existan. El contexto se registrará con `AddDbContext` después de generar el esquema Database First. `ILogger<T>` lo proporciona el host ASP.NET Core; no necesita un registro manual.
