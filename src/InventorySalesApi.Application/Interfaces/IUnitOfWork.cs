using System.Threading;
using System.Threading.Tasks;

namespace InventorySalesApi.Application.Interfaces;

/// <summary>
/// Interfaz para gestionar las transacciones de base de datos a nivel de caso de uso.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos de manera atómica.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
