using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Domain.Entities;

namespace InventorySalesApi.Application.Interfaces;

/// <summary>
/// Contrato para las operaciones de consulta y persistencia de Categorías.
/// </summary>
public interface ICategoriaRepository
{
    Task<Categoria?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Categoria>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task AgregarAsync(Categoria categoria, CancellationToken cancellationToken = default);
}
