using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using InventorySalesApi.Domain.Enums;

namespace InventorySalesApi.UnitTests;

public class VentaDomainTest
{
    [Fact]
    public void Constructor_DatosValidos_DebeCrearVentaPendienteConTotalCero()
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var venta = new Venta(clienteId);

        // Assert
        venta.ClienteId.Should().Be(clienteId);
        venta.Estado.Should().Be(EstadoVenta.Pendiente);
        venta.Total.Amount.Should().Be(0.00m);
        venta.Detalles.Should().BeEmpty();
    }

    [Fact]
    public void AgregarDetalle_VentaPendiente_DebeAgregarYRecalcularTotal()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var venta = new Venta(clienteId);
        var producto = new Producto("PROD-1", "Mouse Gamer", "USB", new Money(30.00m), 10, Guid.NewGuid());
        var usuarioId = Guid.NewGuid();

        // Act
        venta.AgregarDetalle(producto, 2, 5.00m, usuarioId); // 2 unidades * $30.00 - $5.00 desc = $55.00

        // Assert
        venta.Detalles.Should().HaveCount(1);
        venta.Total.Amount.Should().Be(55.00m);
        producto.Stock.Should().Be(8); // 10 - 2 = 8
    }

    [Fact]
    public void CompletarVenta_VentaConItems_DebeCambiarEstadoACompletada()
    {
        // Arrange
        var venta = new Venta(Guid.NewGuid());
        var producto = new Producto("PROD-1", "Mouse Gamer", "USB", new Money(30.00m), 10, Guid.NewGuid());
        venta.AgregarDetalle(producto, 1, 0, Guid.NewGuid());

        // Act
        venta.CompletarVenta();

        // Assert
        venta.Estado.Should().Be(EstadoVenta.Completada);
    }

    [Fact]
    public void CancelarVenta_VentaActiva_DebeCambiarEstadoACanceladaYReintegrarStock()
    {
        // Arrange
        var venta = new Venta(Guid.NewGuid());
        var producto = new Producto("PROD-1", "Mouse Gamer", "USB", new Money(30.00m), 10, Guid.NewGuid());
        var usuarioId = Guid.NewGuid();
        venta.AgregarDetalle(producto, 3, 0, usuarioId); // stock queda en 7

        // Act
        venta.CancelarVenta(usuarioId);

        // Assert
        venta.Estado.Should().Be(EstadoVenta.Cancelada);
        producto.Stock.Should().Be(10); // 7 + 3 = 10 (reintegrado)
    }
}
