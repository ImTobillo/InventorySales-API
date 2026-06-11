using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Domain.Entities;

namespace InventorySalesApi.Application.Interfaces;

/// <summary>
/// Contrato para las operaciones de persistencia del agregado Producto.
/// </summary>
public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Producto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Producto?> ObtenerPorSkuAsync(string sku, CancellationToken cancellationToken = default);
    Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default);
    void Actualizar(Producto producto);
    void Eliminar(Producto producto);
}
