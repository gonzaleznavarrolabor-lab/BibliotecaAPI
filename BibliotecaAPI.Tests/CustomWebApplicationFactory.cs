using BibliotecaAPI.Data;
using BibliotecaAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Data.Sqlite;

namespace BibliotecaAPI.Tests;
public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<BibliotecaContext>();

            db.Database.EnsureCreated();

            SeedData(db);
        });
    }

    private void SeedData(BibliotecaContext context)
    {
        // AUTORES (igual que el script del manual)
        var autores = new[]
        {
            new Autor
            {
                Nombre = "J. K. Rowling",
                Nacionalidad = "Reino Unido",
                FechaNacimiento = new DateTime(1965, 7, 31)
            },
            new Autor
            {
                Nombre = "George R. R. Martin",
                Nacionalidad = "Estados Unidos",
                FechaNacimiento = new DateTime(1948, 9, 20)
            },
            new Autor
            {
                Nombre = "Brandon Sanderson",
                Nacionalidad = "Estados Unidos",
                FechaNacimiento = new DateTime(1975, 12, 19)
            },
            new Autor
            {
                Nombre = "Stephen King",
                Nacionalidad = "Estados Unidos",
                FechaNacimiento = new DateTime(1947, 9, 21)
            },
            new Autor
            {
                Nombre = "J. R. R. Tolkien",
                Nacionalidad = "Reino Unido",
                FechaNacimiento = new DateTime(1892, 1, 3)
            }
        };
        context.Autores.AddRange(autores);
        context.SaveChanges();

        // LIBROS (igual que el script del manual)
        var libros = new[]
        {
            // J. K. Rowling (AutorId = 1)
            new Libro
            {
                Titulo = "Harry Potter y la piedra filosofal",
                Genero = "Fantasia",
                NumeroPaginas = 320,
                Precio = 22.50m,
                Disponible = true,
                FechaPublicacion = new DateTime(1997, 6, 26),
                AutorId = autores[0].Id
            },
            new Libro
            {
                Titulo = "Harry Potter y la cámara secreta",
                Genero = "Fantasia",
                NumeroPaginas = 341,
                Precio = 24.95m,
                Disponible = true,
                FechaPublicacion = new DateTime(1998, 7, 2),
                AutorId = autores[0].Id
            },
            new Libro
            {
                Titulo = "Harry Potter y el prisionero de Azkaban",
                Genero = "Fantasia",
                NumeroPaginas = 435,
                Precio = 27.50m,
                Disponible = false,
                FechaPublicacion = new DateTime(1999, 7, 8),
                AutorId = autores[0].Id
            },
            // George R. R. Martin (AutorId = 2)
            new Libro
            {
                Titulo = "Juego de Tronos",
                Genero = "Fantasia",
                NumeroPaginas = 870,
                Precio = 39.95m,
                Disponible = true,
                FechaPublicacion = new DateTime(1996, 8, 6),
                AutorId = autores[1].Id
            },
            new Libro
            {
                Titulo = "Choque de Reyes",
                Genero = "Fantasia",
                NumeroPaginas = 960,
                Precio = 42.00m,
                Disponible = false,
                FechaPublicacion = new DateTime(1998, 11, 16),
                AutorId = autores[1].Id
            },
            new Libro
            {
                Titulo = "Tormenta de Espadas",
                Genero = "Fantasia",
                NumeroPaginas = 1210,
                Precio = 45.90m,
                Disponible = true,
                FechaPublicacion = new DateTime(2000, 8, 8),
                AutorId = autores[1].Id
            },
            // Brandon Sanderson (AutorId = 3)
            new Libro
            {
                Titulo = "Elantris",
                Genero = "Fantasia",
                NumeroPaginas = 650,
                Precio = 18.95m,
                Disponible = true,
                FechaPublicacion = new DateTime(2005, 4, 21),
                AutorId = autores[2].Id
            },
            new Libro
            {
                Titulo = "Nacidos de la Bruma",
                Genero = "Fantasia",
                NumeroPaginas = 720,
                Precio = 29.90m,
                Disponible = true,
                FechaPublicacion = new DateTime(2006, 7, 17),
                AutorId = autores[2].Id
            },
            new Libro
            {
                Titulo = "El Camino de los Reyes",
                Genero = "Fantasia",
                NumeroPaginas = 1250,
                Precio = 44.95m,
                Disponible = false,
                FechaPublicacion = new DateTime(2010, 8, 31),
                AutorId = autores[2].Id
            },
            // Stephen King (AutorId = 4)
            new Libro
            {
                Titulo = "It",
                Genero = "Terror",
                NumeroPaginas = 1138,
                Precio = 34.90m,
                Disponible = true,
                FechaPublicacion = new DateTime(1986, 9, 15),
                AutorId = autores[3].Id
            },
            new Libro
            {
                Titulo = "El Resplandor",
                Genero = "Terror",
                NumeroPaginas = 600,
                Precio = 26.95m,
                Disponible = true,
                FechaPublicacion = new DateTime(1977, 1, 28),
                AutorId = autores[3].Id
            },
            new Libro
            {
                Titulo = "Misery",
                Genero = "Suspense",
                NumeroPaginas = 480,
                Precio = 21.90m,
                Disponible = false,
                FechaPublicacion = new DateTime(1987, 6, 8),
                AutorId = autores[3].Id
            },
            // J. R. R. Tolkien (AutorId = 5)
            new Libro
            {
                Titulo = "El Señor de los Anillos",
                Genero = "Fantasia",
                NumeroPaginas = 1390,
                Precio = 54.95m,
                Disponible = true,
                FechaPublicacion = new DateTime(1954, 7, 29),
                AutorId = autores[4].Id
            },
            new Libro
            {
                Titulo = "El Hobbit",
                Genero = "Fantasia",
                NumeroPaginas = 310,
                Precio = 18.50m,
                Disponible = true,
                FechaPublicacion = new DateTime(1937, 9, 21),
                AutorId = autores[4].Id
            },
            new Libro
            {
                Titulo = "El Silmarillion",
                Genero = "Fantasia",
                NumeroPaginas = 450,
                Precio = 25.50m,
                Disponible = false,
                FechaPublicacion = new DateTime(1977, 9, 15),
                AutorId = autores[4].Id
            }
        };
        context.Libros.AddRange(libros);
        context.SaveChanges();
    }
}