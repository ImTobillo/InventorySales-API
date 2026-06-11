using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public class MovimientoStockRepository : IMovimientoStockRepository
{
    private readonly AppDbContext _context;

    public MovimientoStockRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MovimientoStock>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.MovimientosStock
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public async Task<IReadOnlyList<MovimientoStock>> ObtenerPorProductoIdAsync(Guid productoId, CancellationToken cancellationToken = default)
    {
        var list = await _context.MovimientosStock
            .Include(m => m.Producto)
            .Include(m => m.Usuario)
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.Fecha)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public async Task AgregarAsync(MovimientoStock movimiento, CancellationToken cancellationToken = default)
    {
        await _context.MovimientosStock.AddAsync(movimiento, cancellationToken);
    }
}
