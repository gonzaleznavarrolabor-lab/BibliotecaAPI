using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using BibliotecaAPI.DTOs;
using FluentAssertions;
using Xunit;

namespace BibliotecaAPI.Tests;

public class AutorControllerTests
{
    private readonly HttpClient _client;

    public AutorControllerTests()
    {
        var factory = new CustomWebApplicationFactory();
        _client = factory.CreateClient();
    }

    // ---------------------------------------------------------------
    // GET - Obtener todos los autores
    // ---------------------------------------------------------------

    [Fact]
    public async Task ObtenerTodosDeberiaDevolverOkConLosAutoresSemilla()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/Autor");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        List<AutorDTO>? autores =
            await response.Content.ReadFromJsonAsync<List<AutorDTO>>();

        autores.Should().NotBeNull();
        autores!.Count.Should().BeGreaterOrEqualTo(5);
        autores.Should().Contain(a => a.Nombre == "J. K. Rowling");
        autores.Should().Contain(a => a.Nombre == "J. R. R. Tolkien");
    }

    // ---------------------------------------------------------------
    // GET - Obtener autor por ID
    // ---------------------------------------------------------------

    [Fact]
    public async Task ObtenerPorIdConIdExistenteDeberiaDevolverElAutor()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/Autor/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AutorDTO? autor = await response.Content.ReadFromJsonAsync<AutorDTO>();

        autor.Should().NotBeNull();
        autor!.Id.Should().Be(1);
        autor.Nombre.Should().Be("J. K. Rowling");
        autor.Nacionalidad.Should().Be("Reino Unido");
    }

    [Fact]
    public async Task ObtenerPorId_ConIdInexistente_DeberiaDevolver404()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/Autor/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------
    // POST - Crear autor
    // ---------------------------------------------------------------

    [Fact]
    public async Task Crear_UnAutor_DeberiaPersistirloEnBaseDeDatos()
    {
        // Arrange
        var nuevoAutor = new AutorCreateDTO
        {
            Nombre = $"Autor Test {Guid.NewGuid()}",
            Nacionalidad = "España",
            FechaNacimiento = new DateTime(1980, 5, 10)
        };

        // Act
        HttpResponseMessage postResponse =
            await _client.PostAsJsonAsync("/api/Autor", nuevoAutor);

        // Assert: código HTTP correcto
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert: el autor existe realmente en la base de datos
        List<AutorDTO>? todos =
            await _client.GetFromJsonAsync<List<AutorDTO>>("/api/Autor");

        todos.Should().Contain(a =>
            a.Nombre == nuevoAutor.Nombre &&
            a.Nacionalidad == nuevoAutor.Nacionalidad);
    }

    // ---------------------------------------------------------------
    // PUT - Modificar autor
    // ---------------------------------------------------------------

    [Fact]
    public async Task Actualizar_UnAutor_DeberiaGuardarLosCambios()
    {
        // Arrange: se crea un autor propio para no interferir con otros tests
        string nombre = $"Autor Update {Guid.NewGuid()}";

        await _client.PostAsJsonAsync("/api/Autor", new AutorCreateDTO
        {
            Nombre = nombre,
            Nacionalidad = "Francia",
            FechaNacimiento = new DateTime(1970, 1, 1)
        });

        List<AutorDTO> todos =
            (await _client.GetFromJsonAsync<List<AutorDTO>>("/api/Autor"))!;

        AutorDTO creado = todos.Single(a => a.Nombre == nombre);

        var autorActualizado = new AutorUpdateDTO
        {
            Nombre = nombre,
            Nacionalidad = "Italia",
            FechaNacimiento = new DateTime(1970, 1, 1)
        };

        // Act
        HttpResponseMessage putResponse =
            await _client.PutAsJsonAsync($"/api/Autor/{creado.Id}", autorActualizado);

        // Assert: código HTTP correcto
        putResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert: el cambio se ha guardado en base de datos
        AutorDTO? recargado =
            await _client.GetFromJsonAsync<AutorDTO>($"/api/Autor/{creado.Id}");

        recargado!.Nacionalidad.Should().Be("Italia");
    }

    [Fact]
    public async Task Actualizar_ConIdInexistente_DeberiaDevolver404()
    {
        var autorActualizado = new AutorUpdateDTO
        {
            Nombre = "No existe",
            Nacionalidad = "Ninguna",
            FechaNacimiento = new DateTime(2000, 1, 1)
        };

        HttpResponseMessage response =
            await _client.PutAsJsonAsync("/api/Autor/999999", autorActualizado);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------
    // DELETE - Eliminar autor
    // ---------------------------------------------------------------

    [Fact]
    public async Task Eliminar_UnAutor_DeberiaBorrarloDeBaseDeDatos()
    {
        // Arrange
        string nombre = $"Autor Delete {Guid.NewGuid()}";

        await _client.PostAsJsonAsync("/api/Autor", new AutorCreateDTO
        {
            Nombre = nombre,
            Nacionalidad = "Portugal",
            FechaNacimiento = new DateTime(1990, 3, 3)
        });

        List<AutorDTO> todos =
            (await _client.GetFromJsonAsync<List<AutorDTO>>("/api/Autor"))!;

        AutorDTO creado = todos.Single(a => a.Nombre == nombre);

        // Act
        HttpResponseMessage deleteResponse =
            await _client.DeleteAsync($"/api/Autor/{creado.Id}");

        // Assert: código HTTP correcto
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert: ya no existe en base de datos
        HttpResponseMessage getResponse =
            await _client.GetAsync($"/api/Autor/{creado.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Eliminar_ConIdInexistente_DeberiaDevolver404()
    {
        HttpResponseMessage response =
            await _client.DeleteAsync("/api/Autor/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------------------------------------------------------------
    // CONSULTAS ADICIONALES (LAS QUE YA TENÍAS)
    // ---------------------------------------------------------------

    [Fact]
    public async Task ObtenerPorNacionalidadDeberiaFiltrarCorrectamente()
    {
        List<AutorDTO>? autores =
            await _client.GetFromJsonAsync<List<AutorDTO>>(
                $"/api/Autor/nacionalidad/{Uri.EscapeDataString("Reino Unido")}");

        autores.Should().NotBeNull();
        autores!.Should().OnlyContain(a => a.Nacionalidad == "Reino Unido");
        autores.Should().Contain(a => a.Nombre == "J. K. Rowling");
        autores.Should().Contain(a => a.Nombre == "J. R. R. Tolkien");
    }

    [Fact]
    public async Task ObtenerSinLibros_NoDeberiaContenerAutoresConLibros()
    {
        List<AutorDTO>? autores =
            await _client.GetFromJsonAsync<List<AutorDTO>>("/api/Autor/sin-libros");

        autores.Should().NotBeNull();
        // Los 5 autores semilla tienen libros asociados, por lo que no deben aparecer
        autores!.Should().NotContain(a => a.Nombre == "J. K. Rowling");
        autores.Should().NotContain(a => a.Nombre == "J. R. R. Tolkien");
    }

    [Fact]
    public async Task ObtenerAutorConMasLibrosDeberiaDevolverUnAutorConVariosLibros()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/Autor/mas-libros");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AutorDTO? autor = await response.Content.ReadFromJsonAsync<AutorDTO>();

        autor.Should().NotBeNull();
        autor!.Libros.Count.Should().BeGreaterOrEqualTo(3);
    }
}