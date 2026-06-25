using System;
using FluentAssertions;
using Xunit;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using InventorySalesApi.Domain.Enums;

namespace InventorySalesApi.UnitTests;

public class ProductoDomainTest
{
    [Fact]
    public void Constructor_DatosValidos_DebeCrearProductoCorrectamente()
    {
        // Arrange
        var sku = "PROD-ABC-123";
        var nombre = "Notebook Intel i5";
        var descripcion = "8GB RAM, 256GB SSD";
        var precio = new Money(850.00m);
        var stockInicial = 15;
        var categoriaId = Guid.NewGuid();

        // Act
        var producto = new Producto(sku, nombre, descripcion, precio, stockInicial, categoriaId);

        // Assert
        producto.Sku.Should().Be("PROD-ABC-123");
        producto.Nombre.Should().Be("Notebook Intel i5");
        producto.Descripcion.Should().Be("8GB RAM, 256GB SSD");
        producto.Precio.Amount.Should().Be(850.00m);
        producto.Stock.Should().Be(15);
        producto.CategoriaId.Should().Be(categoriaId);
    }

    [Fact]
    public void Constructor_PrecioNegativo_DebeLanzarDomainException()
    {
        // Arrange & Act
        Action act = () => new Money(-10.00m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("El monto no puede ser negativo.");
    }

    [Fact]
    public void RegistrarMovimientoStock_IngresoValido_DebeIncrementarStock()
    {
        // Arrange
        var producto = new Producto("SKU-1", "Nombre", "Desc", new Money(10.00m), 10, Guid.NewGuid());
        var usuarioId = Guid.NewGuid();

        // Act
        producto.RegistrarMovimientoStock(5, TipoMovimiento.Ingreso, "Entrada de prueba", usuarioId);

        // Assert
        producto.Stock.Should().Be(15);
        producto.MovimientosStock.Should().HaveCount(1);
    }

    [Fact]
    public void RegistrarMovimientoStock_EgresoValido_DebeDecrementarStock()
    {
        // Arrange
        var producto = new Producto("SKU-1", "Nombre", "Desc", new Money(10.00m), 10, Guid.NewGuid());
        var usuarioId = Guid.NewGuid();

        // Act
        producto.RegistrarMovimientoStock(4, TipoMovimiento.Egreso, "Salida de prueba", usuarioId);

        // Assert
        producto.Stock.Should().Be(6);
        producto.MovimientosStock.Should().HaveCount(1);
    }

    [Fact]
    public void RegistrarMovimientoStock_StockInsuficiente_DebeLanzarDomainException()
    {
        // Arrange
        var producto = new Producto("SKU-1", "Teclado Mecánico", "Desc", new Money(10.00m), 3, Guid.NewGuid());
        var usuarioId = Guid.NewGuid();

        // Act
        Action act = () => producto.RegistrarMovimientoStock(10, TipoMovimiento.Egreso, "Salida de prueba", usuarioId);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Stock insuficiente para el producto 'Teclado Mecánico' (SKU: SKU-1). Disponible: 3, Solicitado: 10.");
    }
}
