using System;
using System.Collections.Generic;
using InventorySalesApi.Domain.Common;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using InventorySalesApi.Domain.Enums;

namespace InventorySalesApi.Domain.Entities;

/// <summary>
/// Representa un producto en el inventario.
/// </summary>
public class Producto : BaseEntity, IAggregateRoot
{
    private readonly List<MovimientoStock> _movimientosStock = new();
    private readonly List<DetalleVenta> _detallesVenta = new();

    public string Sku { get; private set; } = null!;
    public string Nombre { get; private set; } = null!;
    public string? Descripcion { get; private set; }
    public Money Precio { get; private set; }
    public int Stock { get; private set; }
    public Guid CategoriaId { get; private set; }

    // Propiedades de navegación
    public virtual Categoria Categoria { get; private set; } = null!;
    public virtual IReadOnlyCollection<MovimientoStock> MovimientosStock => _movimientosStock.AsReadOnly();
    public virtual IReadOnlyCollection<DetalleVenta> DetallesVenta => _detallesVenta.AsReadOnly();

    // Constructor privado requerido por EF Core
    private Producto() : base()
    {
    }

    /// <summary>
    /// Crea un nuevo producto con validaciones iniciales de negocio.
    /// </summary>
    public Producto(string sku, string nombre, string? descripcion, Money precio, int stockInicial, Guid categoriaId) : base()
    {
        ValidarDatos(sku, nombre, precio, stockInicial, categoriaId);

        Sku = sku.Trim().ToUpper();
        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
        Precio = precio;
        Stock = stockInicial;
        CategoriaId = categoriaId;
    }

    /// <summary>
    /// Actualiza la información básica del producto.
    /// </summary>
    public void Actualizar(string nombre, string? descripcion, Money precio, Guid categoriaId)
    {
        ValidarDatos(Sku, nombre, precio, Stock, categoriaId);

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
        Precio = precio;
        CategoriaId = categoriaId;
    }

    /// <summary>
    /// Registra una modificación en el inventario encapsulando las reglas de stock.
    /// </summary>
    /// <param name="cantidad">La cantidad de variación (debe ser positiva excepto en Ajuste si disminuye stock).</param>
    /// <param name="tipo">Tipo de movimiento (Ingreso, Egreso, Ajuste).</param>
    /// <param name="descripcion">Razón del movimiento.</param>
    /// <param name="usuarioId">ID del usuario que realiza la operación.</param>
    public void RegistrarMovimientoStock(int cantidad, TipoMovimiento tipo, string descripcion, Guid usuarioId)
    {
        if (cantidad == 0)
        {
            throw new DomainException("La cantidad para el movimiento de stock no puede ser cero.");
        }

        if (usuarioId == Guid.Empty)
        {
            throw new DomainException("El ID de usuario que registra el movimiento es requerido.");
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new DomainException("Debe proporcionar una descripción o justificación para el movimiento de stock.");
        }

        int cantidadAbsoluta = Math.Abs(cantidad);

        switch (tipo)
        {
            case TipoMovimiento.Ingreso:
                Stock += cantidadAbsoluta;
                break;

            case TipoMovimiento.Egreso:
                if (Stock < cantidadAbsoluta)
                {
                    throw new DomainException($"Stock insuficiente para el producto '{Nombre}' (SKU: {Sku}). Disponible: {Stock}, Solicitado: {cantidadAbsoluta}.");
                }
                Stock -= cantidadAbsoluta;
                break;

            case TipoMovimiento.Ajuste:
                // En un ajuste, 'cantidad' puede ser positiva (ingreso) o negativa (egreso)
                if (Stock + cantidad < 0)
                {
                    throw new DomainException($"El ajuste de stock ({cantidad}) no es válido porque resultaría en un stock negativo ({Stock + cantidad}) para el producto '{Nombre}'.");
                }
                Stock += cantidad;
                break;

            default:
                throw new DomainException("Tipo de movimiento de stock no soportado.");
        }

        // Se crea el movimiento y se asocia al producto
        var movimiento = new MovimientoStock(Id, cantidad, tipo, descripcion, usuarioId);
        _movimientosStock.Add(movimiento);
    }

    private static void ValidarDatos(string sku, string nombre, Money precio, int stock, Guid categoriaId)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("El SKU del producto es requerido.");
        }

        if (sku.Length > 50)
        {
            throw new DomainException("El SKU no puede superar los 50 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainException("El nombre del producto no puede estar vacío.");
        }

        if (nombre.Length > 150)
        {
            throw new DomainException("El nombre del producto no puede superar los 150 caracteres.");
        }

        if (stock < 0)
        {
            throw new DomainException("El stock inicial de un producto no puede ser negativo.");
        }

        if (categoriaId == Guid.Empty)
        {
            throw new DomainException("Debe asociar el producto a una categoría válida.");
        }
    }
}
