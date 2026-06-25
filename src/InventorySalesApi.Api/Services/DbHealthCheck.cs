using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Infrastructure.Persistence;

namespace InventorySalesApi.Api.Services;

public class DbHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public DbHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _context.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Healthy("La base de datos SQL Server está accesible.");
            }
            return HealthCheckResult.Unhealthy("No se puede conectar a la base de datos SQL Server.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error al verificar la salud de la base de datos.", ex);
        }
    }
}
