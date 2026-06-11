using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Domain.Entities;

namespace InventorySalesApi.Application.Interfaces;

/// <summary>
/// Contrato para las operaciones de persistencia del agregado Cliente.
/// </summary>
public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cliente>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Cliente?> ObtenerPorDocumentoAsync(string documentoIdentidad, CancellationToken cancellationToken = default);
    Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AgregarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    void Actualizar(Cliente cliente);
}
