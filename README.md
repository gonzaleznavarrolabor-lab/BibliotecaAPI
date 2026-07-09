# BibliotecaAPI — Tests de integración

Este repositorio contiene:

- `BibliotecaAPI/` → la API ASP.NET Core proporcionada (Controllers, Services, DTOs, EF Core).
- `BibliotecaAPI.Tests/` → el proyecto de tests de integración (xUnit + WebApplicationFactory + FluentAssertions).
- `Scripts/DatosIniciales.sql` → script de datos iniciales (autores y libros semilla).

## Pasos para ejecutar

1. **Abrir la solución** `BibliotecaAPI.slnx` en Visual Studio.

2. **Restaurar / crear la base de datos** (Consola del Administrador de paquetes, proyecto por defecto = BibliotecaAPI):

   ```powershell
   Add-Migration InitialCreate   # solo si no existe ya la migración
   Update-Database
   ```

3. **Insertar los datos iniciales**: abrir una nueva consulta SQL sobre `BibliotecaDB`
   (SQL Server Object Explorer → (localdb)\MSSQLLocalDB → Databases → BibliotecaDB → New Query)
   y ejecutar el contenido de `Scripts/DatosIniciales.sql`.

4. **Ejecutar los tests**: Test Explorer → Run All, o bien:

   ```powershell
   dotnet test
   ```

   Los tests levantan la API en memoria con `WebApplicationFactory<Program>` y hacen
   peticiones HTTP reales contra ella, que a su vez usa Entity Framework Core contra la
   base de datos LocalDB real `BibliotecaDB` (la misma que se acaba de poblar).

## Notas de diseño

- `CustomWebApplicationFactory` no sustituye el `DbContext` ni la cadena de conexión:
  se usa la configuración de `appsettings.json` tal cual, porque el ejercicio pide
  expresamente pruebas contra una base de datos SQL Server LocalDB real (no InMemory).
- Los tests de creación/actualización/eliminación generan datos con un `Guid` en el
  nombre/título para no colisionar entre ejecuciones ni con los datos semilla.
- Se ha añadido `InternalsVisibleTo` en `BibliotecaAPI.csproj` para que el proyecto de
  tests pueda referenciar la clase `Program` (generada como `internal` al usar
  top-level statements).
