# VENTA-STOCK

Base del proyecto final: ASP.NET Core MVC en .NET 10 y SQL Server, con Entity Framework Core Database First.

## Proyectos

- **VentaStock.Domain**: entidades, reglas de negocio y contratos de repositorios (`Contracts/`).
- **VentaStock.Application**: interfaces de servicios (`Contracts/`) e implementaciones (`Services/`).
- **VentaStock.Infrastructure**: contexto generado desde SQL Server y repositorios EF Core.
- **VentaStock.Web**: presentación MVC y registro de dependencias.

La aplicación incluye autenticación por cookies, administración de usuarios, categorías, servicios, productos, stock, clientes y un panel de módulos con accesos según permisos. Las constantes de roles y permisos se centralizan en `VentaStock.Domain/Security`.

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
- Se impide eliminar un producto con existencias positivas o negativas. Los productos con movimientos de stock tampoco se pueden eliminar, aunque tengan cantidad 0; las futuras ventas deberán preservar también su historial.

Las existencias se modifican desde el módulo Stock, mediante ingresos o ajustes; se admiten cantidades negativas y unidades enteras. Después de actualizar los permisos, cerrar sesión y volver a ingresar para renovar los claims.

## Stock

El landing incluye el módulo Stock. Su grilla permite buscar por nombre/código y filtrar existencias en el mínimo o por debajo, con una alerta diferenciada para valores negativos. Los permisos `Stock.Read`, `Stock.Receipt`, `Stock.Adjust` y `Stock.ReadMovements` son independientes. Quien opera o consulta historial recibe la identificación y las existencias necesarias dentro de ese módulo, sin requerir permisos del catálogo ni recibir acceso a precios.

| Rol | Consulta | Ingresos | Ajustes | Historial |
|---|---|---|---|---|
| Dueño | Sí | Sí | Sí | Sí |
| Gestor de Stock | Sí | Sí | Sí | Sí |
| Gestor de Productos | Sí | No | No | No |
| Vendedor | Sí | No | No | No |

- **Ingreso:** cantidad entera positiva y observación opcional; suma sobre el saldo actual, sin restablecer negativos a 0.
- **Ajuste:** cantidad real contada, entera no negativa, y motivo obligatorio. Registra la diferencia; si coincide con el saldo, avisa sin crear un movimiento vacío. Si el saldo cambió desde que se abrió el formulario, exige revisar el valor actualizado y confirmar nuevamente.
- **Historial:** muestra tipo, usuario, fecha UTC, motivo, variación y cantidades anterior/posterior, en páginas de 50 movimientos. No ofrece edición ni eliminación. Las correcciones se registran con nuevos movimientos.
- **Guardado:** la modificación del saldo y el movimiento se confirman en una única transacción con bloqueo de la fila de stock. Cada formulario tiene un identificador único para evitar aplicar dos veces el mismo envío.
- **Conservación:** los movimientos impiden eliminar el producto. Si se elimina el usuario, se conserva su nombre en el movimiento y se retira únicamente el vínculo con la cuenta.

Compras, proveedores y descuentos de stock por ventas quedan para tareas posteriores. Las ventas deberán generar su movimiento y actualizar stock dentro de la misma transacción que la venta.

### Verificación manual pendiente

1. Cerrar sesión y volver a ingresar para renovar los permisos. Revisar que Dueño/Gestor de Stock tengan acciones y que Vendedor/Gestor de Productos solo consulten, también al intentar acceder directamente a las URLs de escritura o historial.
2. Ingresar 10 unidades en un producto sin precios; comprobar el saldo y un solo movimiento con actor, fecha y cantidades correctos. Reenviar exactamente el mismo formulario y verificar que no se duplique.
3. Ajustar de 10 a 7 con motivo; comprobar variación −3. Rechazar cantidad contada negativa, ingreso 0, fracciones y ajuste sin motivo. Un ajuste a la cantidad actual no genera movimiento.
4. Abrir dos formularios: registrar un ingreso en uno e intentar confirmar un conteo abierto previamente en el otro. Debe avisar que cambió el saldo antes de permitir reconfirmar.
5. Registrar ingresos simultáneos de distintas cantidades; comprobar que ambos se sumen y que cada movimiento encadene su saldo con el anterior. En un producto de prueba con saldo negativo previo, verificar que ingresar 10 sobre −3 deje 7.
6. Revisar alertas al mínimo y por debajo; intentar eliminar un producto con historial después de ajustar sus existencias a 0. Debe rechazarse.

## Clientes

El módulo incluye listado, búsqueda, alta y modificación. Nombre y apellido son obligatorios; dirección y teléfono son opcionales y pueden repetirse. No se almacenan correos. No hay eliminación ni campos de saldo inicial o límite de deuda en esta entrega.

- Documentos: DNI, CUIL y CUIT. El número es único incluso si se selecciona otro tipo de documento: un CUIL y un CUIT con el mismo número no generan dos clientes.
- DNI: hasta 8 dígitos, distinto de cero. Se aceptan puntos y espacios; se guarda con 8 dígitos, completando ceros a la izquierda para impedir duplicados por formato.
- CUIL/CUIT: 11 dígitos con o sin guiones, validación de prefijo y dígito verificador módulo 11 en el servidor, tanto al crear como al editar. Se guardan sin guiones. La validación se centraliza en `CustomerDocumentValidator` y se reutiliza desde DTOs y servicio.
- Cuenta corriente: exige CUIL o CUIT válido, incluso al cambiar un cliente previamente de contado. Esto clasifica al cliente; la cuenta financiera, los saldos y sus movimientos se implementarán en T15. Antes de habilitar saldos, T15 debe agregar la comprobación transaccional de saldo cero para volver a Contado, conservando historial.
- Búsqueda: nombre, apellido, documento o teléfono. La edición utiliza `rowversion` para rechazar sobrescrituras cuando otro usuario modificó al mismo cliente.

| Rol | Consulta | Alta de contado | Alta de cuenta corriente | Modificación |
|---|---|---|---|---|
| Dueño | Sí | Sí | Sí | Sí |
| Gestor de Clientes | Sí | Sí | Sí | Sí |
| Vendedor | Sí | Sí | No | No |

Los permisos son `Customers.Read`, `Customers.Create`, `Customers.CreateOnAccount` y `Customers.Update`. Crear un cliente de cuenta corriente requiere ambos permisos de alta. Pasar de Contado a Cuenta corriente requiere edición y `Customers.CreateOnAccount`. Estos permisos no conceden acceso a saldos ni movimientos financieros.

La validación es local: no consulta padrones ni confirma titularidad o inscripción. Se consultaron como referencia para la estructura y los ponderadores la [implementación de python-stdnum](https://github.com/arthurdejong/python-stdnum/blob/master/stdnum/ar/cuit.py) y la [ayuda de ARCA sobre CUIL/CUIT](https://www.afip.gov.ar/simplificacionregistral/alta-ya/ayuda/documentos/Errores-frecuentes-CUIL-o-CUIT-inexistente.pdf), que distingue la consistencia del número de su existencia en los registros.

### Verificación manual pendiente de Clientes

1. Cerrar sesión y volver a ingresar. Comprobar que el Vendedor consulta ambos tipos, crea solo Contado y no puede editar ni crear Cuenta corriente alterando la solicitud. Dueño/Gestor de Clientes deben poder crear ambos tipos y editar.
2. Crear con DNI y datos de contacto vacíos. Rechazar nombre o apellido vacíos, DNI cero y documentos repetidos. Repetir nombre, apellido o teléfono en otros clientes debe estar permitido.
3. Probar CUIL/CUIT con y sin guiones: deben representar el mismo documento. Rechazar letras, longitud incorrecta, prefijo inválido y dígito verificador incorrecto, tanto en alta como edición.
4. Rechazar Cuenta corriente con DNI. Permitirla con CUIL/CUIT válido, también al cambiar desde Contado. Mientras el cliente sea Cuenta corriente, impedir cambiar su documento a DNI.
5. Buscar por nombre completo, apellido, documento con separadores y teléfono. Revisar formatos de contacto cuando se proporcionan.
6. Abrir la edición en dos pestañas, guardar en una e intentar guardar en la otra. Debe exigir recargar los datos y no sobrescribir la primera modificación.

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
