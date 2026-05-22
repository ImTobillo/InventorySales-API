using System;
using InventorySalesApi.Domain.Common;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.Enums;

namespace InventorySalesApi.Domain.Entities;

/// <summary>
/// Registra el historial de movimientos de inventario (entradas, salidas y ajustes).
/// </summary>
public class MovimientoStock : BaseEntity
{
    public Guid ProductoId { get; private set; }
    public int Cantidad { get; private set; }
    public TipoMovimiento Tipo { get; private set; }
    public DateTimeOffset Fecha { get; private set; }
    public string Descripcion { get; private set; } = null!;
    public Guid UsuarioId { get; private set; }

    // Propiedades de navegación
    public virtual Producto Producto { get; private set; } = null!;
    public virtual Usuario Usuario { get; private set; } = null!;

    // Constructor privado para EF Core
    private MovimientoStock() : base()
    {
    }

    /// <summary>
    /// Crea un registro de movimiento de stock.
    /// Este constructor es invocado principalmente por la entidad Producto para mantener la consistencia.
    /// </summary>
    public MovimientoStock(Guid productoId, int cantidad, TipoMovimiento tipo, string descripcion, Guid usuarioId) : base()
    {
        if (productoId == Guid.Empty)
        {
            throw new DomainException("El ID del producto asociado al movimiento es requerido.");
        }

        if (cantidad == 0)
        {
            throw new DomainException("La cantidad del movimiento no puede ser cero.");
        }

        if (usuarioId == Guid.Empty)
        {
            throw new DomainException("El ID del usuario que registra el movimiento es requerido.");
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new DomainException("La descripción del movimiento es requerida.");
        }

        ProductoId = productoId;
        Cantidad = cantidad;
        Tipo = tipo;
        Fecha = DateTimeOffset.UtcNow;
        Descripcion = descripcion.Trim();
        UsuarioId = usuarioId;
    }
}
