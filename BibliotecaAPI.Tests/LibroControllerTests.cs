using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using BibliotecaAPI.DTOs;
using FluentAssertions;
using Xunit;

namespace BibliotecaAPI.Tests;

/// <summary>
/// Tests de integración del LibroController.
/// Igual que en AutorControllerTests, se usa la base de datos LocalDB real
/// (BibliotecaDB) ya poblada con el script de datos iniciales del manual:
/// 15 libros (Ids 1-15) repartidos entre los 5 autores semilla (Ids 1-5).
/// </summary>
public class LibroControllerTests
{
    private readonly HttpClient _client;

    public LibroControllerTests()
    {
        var factory = new CustomWebApplicationFactory();
        _client = factory.CreateClient();
    }

    // ---------------------------------------------------------------
    // GET
    // ---------------------------------------------------------------

    [Fact]
    public async Task ObtenerTodosDeberiaDevolverOkConLosLibrosSemilla()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Libro");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        List<LibroDTO>? libros =
            await response.Content.ReadFromJsonAsync<List<LibroDTO>>();

        libros.Should().NotBeNull();
        libros!.Count.Should().BeGreaterOrEqualTo(15);
        libros.Should().Contain(l => l.Titulo == "Harry Potter y la piedra filosofal");
    }

    [Fact]
    public async Task ObtenerPorIdConIdExistenteDeberiaDevolverElLibro()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Libro/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        LibroDTO? libro = await response.Content.ReadFromJsonAsync<LibroDTO>();

        libro.Should().NotBeNull();
        libro!.Id.Should().Be(1);
        libro.Titulo.Should().Be("Harry Potter y la piedra filosofal");
        libro.Autor.Should().Be("J. K. Rowling");
    }

    [Fact]
    public async Task ObtenerPorId_ConIdInexistente_DeberiaDevolver404()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Libro/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------
    // POST
    // ---------------------------------------------------------------

    [Fact]
    public async Task Crear_UnLibro_DeberiaPersistirloEnBaseDeDatos()
    {
        // Arrange
        var tituloUnico = $"Libro Test {Guid.NewGuid()}";

        var nuevoLibro = new LibroCreateDTO
        {
            Titulo = tituloUnico,
            Genero = "Ciencia Ficción",
            NumeroPaginas = 300,
            Precio = 19.99m,
            Disponible = true,
            FechaPublicacion = new DateTime(2020, 1, 1),
            AutorId = 1 // J. K. Rowling, autor semilla existente
        };

        // Act
        HttpResponseMessage postResponse =
            await _client.PostAsJsonAsync("/api/Libro", nuevoLibro);

        // Assert: código HTTP correcto
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert: el libro existe realmente en la base de datos
        List<LibroDTO>? encontrados =
            await _client.GetFromJsonAsync<List<LibroDTO>>($"/api/Libro/buscar/{Uri.EscapeDataString(tituloUnico)}");

        encontrados.Should().ContainSingle(l => l.Titulo == tituloUnico);
    }

    // ---------------------------------------------------------------
    // PUT
    // ---------------------------------------------------------------

    [Fact]
    public async Task ActualizarUnLibroDeberiaGuardarLosCambios()
    {
        // Arrange: se crea un libro propio para no interferir con otros tests
        string titulo = $"Libro Update {Guid.NewGuid()}";

        await _client.PostAsJsonAsync("/api/Libro", new LibroCreateDTO
        {
            Titulo = titulo,
            Genero = "Drama",
            NumeroPaginas = 200,
            Precio = 15.00m,
            Disponible = true,
            FechaPublicacion = new DateTime(2015, 6, 1),
            AutorId = 2
        });

        List<LibroDTO> encontrados =
            (await _client.GetFromJsonAsync<List<LibroDTO>>($"/api/Libro/buscar/{Uri.EscapeDataString(titulo)}"))!;

        LibroDTO creado = encontrados.Single(l => l.Titulo == titulo);

        var libroActualizado = new LibroUpdateDTO
        {
            Titulo = titulo,
            Genero = "Drama histórico",
            NumeroPaginas = 250,
            Precio = 22.50m,
            Disponible = false,
            FechaPublicacion = new DateTime(2015, 6, 1),
            AutorId = 2
        };

        // Act
        HttpResponseMessage putResponse =
            await _client.PutAsJsonAsync($"/api/Libro/{creado.Id}", libroActualizado);

        // Assert: código HTTP correcto
        putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert: los cambios se han guardado en base de datos
        LibroDTO? recargado =
            await _client.GetFromJsonAsync<LibroDTO>($"/api/Libro/{creado.Id}");

        recargado!.Genero.Should().Be("Drama histórico");
        recargado.NumeroPaginas.Should().Be(250);
        recargado.Precio.Should().Be(22.50m);
        recargado.Disponible.Should().BeFalse();
    }

    [Fact]
    public async Task Actualizar_ConIdInexistente_DeberiaDevolver404()
    {
        var libroActualizado = new LibroUpdateDTO
        {
            Titulo = "No existe",
            Genero = "Ninguno",
            NumeroPaginas = 1,
            Precio = 1m,
            Disponible = true,
            FechaPublicacion = DateTime.Today,
            AutorId = 1
        };

        HttpResponseMessage response =
            await _client.PutAsJsonAsync("/api/Libro/999999", libroActualizado);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------------

    [Fact]
    public async Task Eliminar_UnLibro_DeberiaBorrarloDeBaseDeDatos()
    {
        // Arrange
        string titulo = $"Libro Delete {Guid.NewGuid()}";

        await _client.PostAsJsonAsync("/api/Libro", new LibroCreateDTO
        {
            Titulo = titulo,
            Genero = "Terror",
            NumeroPaginas = 180,
            Precio = 12.00m,
            Disponible = true,
            FechaPublicacion = new DateTime(2018, 9, 9),
            AutorId = 4
        });

        List<LibroDTO> encontrados =
            (await _client.GetFromJsonAsync<List<LibroDTO>>($"/api/Libro/buscar/{Uri.EscapeDataString(titulo)}"))!;

        LibroDTO creado = encontrados.Single(l => l.Titulo == titulo);

        // Act
        HttpResponseMessage deleteResponse =
            await _client.DeleteAsync($"/api/Libro/{creado.Id}");

        // Assert: código HTTP correcto
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert: ya no existe en base de datos
        HttpResponseMessage getResponse =
            await _client.GetAsync($"/api/Libro/{creado.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Eliminar_ConIdInexistente_DeberiaDevolver404()
    {
        HttpResponseMessage response =
            await _client.DeleteAsync("/api/Libro/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------
    // Consultas
    // ---------------------------------------------------------------

    [Fact]
    public async Task ObtenerPorGeneroDeberiaFiltrarCorrectamente()
    {
        List<LibroDTO>? libros =
            await _client.GetFromJsonAsync<List<LibroDTO>>("/api/Libro/genero/Terror");

        libros.Should().NotBeNull();
        libros!.Should().OnlyContain(l => l.Genero == "Terror");
        libros.Should().Contain(l => l.Titulo == "It");
    }

    [Fact]
    public async Task ObtenerPorAutorDeberiaDevolverSoloLibrosDeEseAutor()
    {
        List<LibroDTO>? libros =
            await _client.GetFromJsonAsync<List<LibroDTO>>("/api/Libro/autor/5");

        libros.Should().NotBeNull();
        libros!.Should().OnlyContain(l => l.AutorId == 5);
        libros.Should().Contain(l => l.Titulo == "El Hobbit");
    }

    [Fact]
    public async Task ObtenerDisponibles_SoloDeberiaDevolverLibrosDisponibles()
    {
        List<LibroDTO>? libros =
            await _client.GetFromJsonAsync<List<LibroDTO>>("/api/Libro/disponibles");

        libros.Should().NotBeNull();
        libros!.Should().OnlyContain(l => l.Disponible);
    }

    [Fact]
    public async Task ObtenerNoDisponiblesSoloDeberiaDevolverLibrosPrestados()
    {
        List<LibroDTO>? libros =
            await _client.GetFromJsonAsync<List<LibroDTO>>("/api/Libro/no-disponibles");

        libros.Should().NotBeNull();
        libros!.Should().OnlyContain(l => !l.Disponible);
        libros.Should().Contain(l => l.Titulo == "Choque de Reyes");
    }

    [Fact]
    public async Task ObtenerBaratosSoloDeberiaDevolverLibrosPorDebajoDelUmbral()
    {
        List<LibroDTO>? libros =
            await _client.GetFromJsonAsync<List<LibroDTO>>("/api/Libro/baratos");

        libros.Should().NotBeNull();
        libros!.Should().OnlyContain(l => l.Precio < 20);
        libros.Should().Contain(l => l.Titulo == "El Hobbit");
    }

    [Fact]
    public async Task ObtenerCarosSoloDeberiaDevolverLibrosPorEncimaDelUmbral()
    {
        List<LibroDTO>? libros =
            await _client.GetFromJsonAsync<List<LibroDTO>>("/api/Libro/caros");

        libros.Should().NotBeNull();
        libros!.Should().OnlyContain(l => l.Precio > 50);
        libros.Should().Contain(l => l.Titulo == "El Señor de los Anillos");
    }

    [Fact]
    public async Task BuscarPorTituloDeberiaEncontrarCoincidenciasParciales()
    {
        List<LibroDTO>? libros =
            await _client.GetFromJsonAsync<List<LibroDTO>>($"/api/Libro/buscar/{Uri.EscapeDataString("Harry Potter")}");

        libros.Should().NotBeNull();
        libros!.Count.Should().BeGreaterOrEqualTo(3);
        libros.Should().OnlyContain(l => l.Titulo.Contains("Harry Potter"));
    }

    [Fact]
    public async Task ObtenerEstadisticasDeberiaDevolverDatosCoherentes()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Libro/estadisticas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        EstadisticasDTO? estadisticas =
            await response.Content.ReadFromJsonAsync<EstadisticasDTO>();

        estadisticas.Should().NotBeNull();
        estadisticas!.TotalLibros.Should().BeGreaterOrEqualTo(15);
        (estadisticas.Disponibles + estadisticas.Prestados)
            .Should().Be(estadisticas.TotalLibros);
        estadisticas.PrecioMedio.Should().BeGreaterThan(0);
        estadisticas.PaginasMedias.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerMasCaroDeberiaDevolverElLibroDeMayorPrecio()
    {
        LibroDTO? libro =
            await _client.GetFromJsonAsync<LibroDTO>("/api/Libro/mas-caro");

        libro.Should().NotBeNull();
        libro!.Titulo.Should().Be("El Señor de los Anillos");
    }

    [Fact]
    public async Task ObtenerMasBaratoDeberiaDevolverElLibroDeMenorPrecio()
    {
        LibroDTO? libro =
            await _client.GetFromJsonAsync<LibroDTO>("/api/Libro/mas-barato");

        libro.Should().NotBeNull();
        libro!.Titulo.Should().Be("El Hobbit");
    }

    // ---------------------------------------------------------------
    // PATCH: prestar / devolver
    // ---------------------------------------------------------------

    [Fact]
    public async Task Prestar_UnLibroDisponible_DeberiaMarcarloComoNoDisponible()
    {
        // Arrange: se crea un libro propio, disponible, para no afectar a otros tests
        string titulo = $"Libro Prestamo {Guid.NewGuid()}";

        await _client.PostAsJsonAsync("/api/Libro", new LibroCreateDTO
        {
            Titulo = titulo,
            Genero = "Aventura",
            NumeroPaginas = 220,
            Precio = 10.00m,
            Disponible = true,
            FechaPublicacion = new DateTime(2019, 4, 4),
            AutorId = 3
        });

        List<LibroDTO> encontrados =
            (await _client.GetFromJsonAsync<List<LibroDTO>>($"/api/Libro/buscar/{Uri.EscapeDataString(titulo)}"))!;

        LibroDTO creado = encontrados.Single(l => l.Titulo == titulo);

        // Act
        HttpResponseMessage patchResponse =
            await _client.PatchAsync($"/api/Libro/{creado.Id}/prestar", null);

        // Assert: código HTTP correcto
        patchResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert: el cambio de estado se ha guardado en base de datos
        LibroDTO? recargado =
            await _client.GetFromJsonAsync<LibroDTO>($"/api/Libro/{creado.Id}");

        recargado!.Disponible.Should().BeFalse();
    }

    [Fact]
    public async Task Devolver_UnLibroPrestado_DeberiaMarcarloComoDisponible()
    {
        // Arrange: se crea un libro propio, ya prestado (Disponible = false)
        string titulo = $"Libro Devolucion {Guid.NewGuid()}";

        await _client.PostAsJsonAsync("/api/Libro", new LibroCreateDTO
        {
            Titulo = titulo,
            Genero = "Aventura",
            NumeroPaginas = 220,
            Precio = 10.00m,
            Disponible = false,
            FechaPublicacion = new DateTime(2019, 4, 4),
            AutorId = 3
        });

        List<LibroDTO> encontrados =
            (await _client.GetFromJsonAsync<List<LibroDTO>>($"/api/Libro/buscar/{Uri.EscapeDataString(titulo)}"))!;

        LibroDTO creado = encontrados.Single(l => l.Titulo == titulo);

        // Act
        HttpResponseMessage patchResponse =
            await _client.PatchAsync($"/api/Libro/{creado.Id}/devolver", null);

        // Assert: código HTTP correcto
        patchResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert: el cambio de estado se ha guardado en base de datos
        LibroDTO? recargado =
            await _client.GetFromJsonAsync<LibroDTO>($"/api/Libro/{creado.Id}");

        recargado!.Disponible.Should().BeTrue();
    }

    [Fact]
    public async Task Prestar_ConIdInexistente_DeberiaDevolver404()
    {
        HttpResponseMessage response =
            await _client.PatchAsync("/api/Libro/999999/prestar", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Devolver_ConIdInexistente_DeberiaDevolver404()
    {
        HttpResponseMessage response =
            await _client.PatchAsync("/api/Libro/999999/devolver", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
