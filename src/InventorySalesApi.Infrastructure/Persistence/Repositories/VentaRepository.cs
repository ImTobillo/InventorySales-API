using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public class VentaRepository : IVentaRepository
{
    private readonly AppDbContext _context;

    public VentaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Venta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Venta>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public async Task AgregarAsync(Venta venta, CancellationToken cancellationToken = default)
    {
        await _context.Ventas.AddAsync(venta, cancellationToken);
    }

    public void Actualizar(Venta venta)
    {
        _context.Ventas.Update(venta);
    }
}
