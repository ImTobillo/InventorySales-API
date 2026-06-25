using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Infrastructure.Persistence;
using InventorySalesApi.Application.Interfaces;

namespace InventorySalesApi.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    public CustomWebApplicationFactory()
    {
        // Establecer variable de entorno para forzar el uso de base de datos en memoria en Program.cs
        Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Sembrar la base de datos en memoria antes de iniciar la prueba
        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AppDbContext>();
                var passwordHasher = scopedServices.GetRequiredService<IPasswordHasher>();

                db.Database.EnsureCreated();

                try
                {
                    DbInitializer.SeedAsync(db, passwordHasher).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();
                    logger.LogError(ex, "Ocurrió un error sembrando los datos en la base de datos de pruebas.");
                }
            }
        });
    }
}
