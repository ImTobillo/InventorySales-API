using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Domain.Entities;

namespace InventorySalesApi.Application.Interfaces;

/// <summary>
/// Contrato para las operaciones de consulta e historial de movimientos de stock.
/// </summary>
public interface IMovimientoStockRepository
{
    Task<IReadOnlyList<MovimientoStock>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MovimientoStock>> ObtenerPorProductoIdAsync(Guid productoId, CancellationToken cancellationToken = default);
    Task AgregarAsync(MovimientoStock movimiento, CancellationToken cancellationToken = default);
}
