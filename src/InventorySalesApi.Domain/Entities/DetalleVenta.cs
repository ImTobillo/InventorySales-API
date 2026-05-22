using System;
using InventorySalesApi.Domain.Common;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;

namespace InventorySalesApi.Domain.Entities;

/// <summary>
/// Representa una línea o ítem de detalle dentro de una venta.
/// </summary>
public class DetalleVenta : BaseEntity
{
    public Guid VentaId { get; private set; }
    public Guid ProductoId { get; private set; }
    public int Cantidad { get; private set; }
    public Money PrecioUnitario { get; private set; }
    public Money Descuento { get; private set; }
    public Money Subtotal { get; private set; }

    // Propiedades de navegación
    public virtual Venta Venta { get; private set; } = null!;
    public virtual Producto Producto { get; private set; } = null!;

    // Constructor privado para EF Core
    private DetalleVenta() : base()
    {
    }

    /// <summary>
    /// Crea una nueva línea de detalle para una venta.
    /// Este constructor es invocado principalmente por la entidad Venta para asegurar la integridad de los datos.
    /// </summary>
    public DetalleVenta(Guid ventaId, Producto producto, int cantidad, decimal descuento) : base()
    {
        if (ventaId == Guid.Empty)
        {
            throw new DomainException("El ID de la venta es requerido.");
        }

        if (producto == null)
        {
            throw new DomainException("El producto para la línea de detalle es requerido.");
        }

        if (cantidad <= 0)
        {
            throw new DomainException($"La cantidad para el producto '{producto.Nombre}' debe ser mayor a cero.");
        }

        if (descuento < 0)
        {
            throw new DomainException("El descuento no puede ser negativo.");
        }

        decimal subtotalBruto = cantidad * producto.Precio.Amount;
        if (descuento > subtotalBruto)
        {
            throw new DomainException($"El descuento (${descuento}) no puede ser mayor que el subtotal bruto (${subtotalBruto}) para '{producto.Nombre}'.");
        }

        VentaId = ventaId;
        ProductoId = producto.Id;
        Producto = producto; // Asignamos la propiedad de navegación para permitir cálculos inmediatos
        Cantidad = cantidad;
        PrecioUnitario = producto.Precio;
        Descuento = new Money(descuento);
        Subtotal = new Money(subtotalBruto - descuento);
    }
}
