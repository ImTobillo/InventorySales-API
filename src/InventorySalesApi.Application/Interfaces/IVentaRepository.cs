using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Domain.Entities;

namespace InventorySalesApi.Application.Interfaces;

/// <summary>
/// Contrato para las operaciones de persistencia del agregado Venta.
/// </summary>
public interface IVentaRepository
{
    Task<Venta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Venta>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(Venta venta, CancellationToken cancellationToken = default);
    void Actualizar(Venta venta);
}
