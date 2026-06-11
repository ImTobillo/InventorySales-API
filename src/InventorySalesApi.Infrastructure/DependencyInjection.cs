using Microsoft.Extensions.DependencyInjection;
using InventorySalesApi.Application.Interfaces;
using Infrastructure.Persistence.Repositories;

namespace InventorySalesApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Registro de la Unidad de Trabajo
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Registro de los Repositorios
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IMovimientoStockRepository, MovimientoStockRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        return services;
    }
}
