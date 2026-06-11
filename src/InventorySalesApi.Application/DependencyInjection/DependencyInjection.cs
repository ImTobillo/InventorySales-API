using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace InventorySalesApi.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Registrar todos los Handlers de MediatR del ensamblado actual
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Registrar todos los Validators de FluentValidation del ensamblado actual
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
