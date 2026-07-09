using BibliotecaAPI.Data;
using Microsoft.EntityFrameworkCore;
using BibliotecaAPI.Interfaces;
using BibliotecaAPI.Services;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsEnvironment("Testing"))
{
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();

    builder.Services.AddSingleton(connection);

    builder.Services.AddDbContext<BibliotecaContext>((sp, options) =>
    {
        options.UseSqlite(sp.GetRequiredService<SqliteConnection>());
    });
}
else
{
    builder.Services.AddDbContext<BibliotecaContext>(options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("BibliotecaConnection"));
    });
}

builder.Services.AddScoped<IAutorService, AutorService>();

builder.Services.AddScoped<ILibroService, LibroService>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
var app = builder.Build();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }