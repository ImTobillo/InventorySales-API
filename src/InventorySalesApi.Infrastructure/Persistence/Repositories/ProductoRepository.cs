using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _context;

    public ProductoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Producto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.Productos
            .Include(p => p.Categoria)
            .ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public async Task<Producto?> ObtenerPorSkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku)) return null;
        var skuUpper = sku.Trim().ToUpper();
        return await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Sku == skuUpper, cancellationToken);
    }

    public async Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        await _context.Productos.AddAsync(producto, cancellationToken);
    }

    public void Actualizar(Producto producto)
    {
        _context.Productos.Update(producto);
    }

    public void Eliminar(Producto producto)
    {
        _context.Productos.Remove(producto);
    }
}
