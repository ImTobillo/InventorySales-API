using System;
using System.Collections.Generic;
using System.Linq;
using InventorySalesApi.Domain.Common;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using InventorySalesApi.Domain.Enums;

namespace InventorySalesApi.Domain.Entities;

/// <summary>
/// Representa una transacción de venta y actúa como Raíz de Agregado (Aggregate Root).
/// </summary>
public class Venta : BaseEntity, IAggregateRoot
{
    private readonly List<DetalleVenta> _detalles = new();

    public DateTimeOffset Fecha { get; private set; }
    public Guid ClienteId { get; private set; }
    public Money Total { get; private set; }
    public EstadoVenta Estado { get; private set; }

    // Propiedades de navegación
    public virtual Cliente Cliente { get; private set; } = null!;
    public virtual IReadOnlyCollection<DetalleVenta> Detalles => _detalles.AsReadOnly();

    // Constructor privado para EF Core
    private Venta() : base()
    {
    }

    /// <summary>
    /// Crea una nueva venta en estado Pendiente.
    /// </summary>
    public Venta(Guid clienteId) : base()
    {
        if (clienteId == Guid.Empty)
        {
            throw new DomainException("El ID del cliente para la venta es requerido.");
        }

        ClienteId = clienteId;
        Fecha = DateTimeOffset.UtcNow;
        Estado = EstadoVenta.Pendiente;
        Total = Money.Zero;
    }

    /// <summary>
    /// Agrega una línea de producto a la venta, valida stock y descuenta la cantidad correspondiente.
    /// </summary>
    /// <param name="producto">El producto a vender.</param>
    /// <param name="cantidad">La cantidad a comprar.</param>
    /// <param name="descuento">Descuento aplicado en dólares/pesos a este ítem.</param>
    /// <param name="usuarioId">ID del usuario que realiza la venta.</param>
    public void AgregarDetalle(Producto producto, int cantidad, decimal descuento, Guid usuarioId)
    {
        if (Estado != EstadoVenta.Pendiente)
        {
            throw new DomainException("Solo se pueden agregar ítems a una venta en estado Pendiente.");
        }

        if (producto == null)
        {
            throw new DomainException("El producto no puede ser nulo.");
        }

        // 1. Descontar el stock mediante la regla de negocio del Producto
        string motivoStock = $"Salida por venta {Id.ToString()[..8]}";
        producto.RegistrarMovimientoStock(cantidad, TipoMovimiento.Egreso, motivoStock, usuarioId);

        // 2. Crear la línea de detalle
        var detalle = new DetalleVenta(Id, producto, cantidad, descuento);
        
        // 3. Añadir a la colección
        _detalles.Add(detalle);

        // 4. Recalcular el total general de la venta
        RecalcularTotal();
    }

    /// <summary>
    /// Finaliza y completa la venta.
    /// </summary>
    public void CompletarVenta()
    {
        if (Estado != EstadoVenta.Pendiente)
        {
            throw new DomainException($"No se puede completar una venta en estado '{Estado}'. Debe estar Pendiente.");
        }

        if (!_detalles.Any())
        {
            throw new DomainException("No se puede completar una venta sin líneas de detalle.");
        }

        Estado = EstadoVenta.Completada;
    }

    /// <summary>
    /// Cancela la venta y devuelve el stock retenido de todos los productos al inventario.
    /// </summary>
    /// <param name="usuarioId">El ID de usuario que ejecuta la cancelación.</param>
    public void CancelarVenta(Guid usuarioId)
    {
        if (Estado == EstadoVenta.Cancelada)
        {
            throw new DomainException("La venta ya se encuentra cancelada.");
        }

        // Si la venta estaba confirmada o pendiente, devolvemos el stock de los productos
        foreach (var detalle in _detalles)
        {
            string motivoReintegro = $"Reintegro de stock por cancelación de venta {Id.ToString()[..8]}";
            detalle.Producto.RegistrarMovimientoStock(detalle.Cantidad, TipoMovimiento.Ingreso, motivoReintegro, usuarioId);
        }

        Estado = EstadoVenta.Cancelada;
    }

    private void RecalcularTotal()
    {
        decimal sumaSubtotales = _detalles.Sum(d => d.Subtotal.Amount);
        Total = new Money(sumaSubtotales);
    }
}
