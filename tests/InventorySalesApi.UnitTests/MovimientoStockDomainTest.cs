using System;
using FluentAssertions;
using Xunit;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.Enums;

namespace InventorySalesApi.UnitTests;

public class MovimientoStockDomainTest
{
    [Fact]
    public void Constructor_DatosValidos_DebeCrearMovimientoCorrectamente()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var cantidad = 10;
        var tipo = TipoMovimiento.Ingreso;
        var descripcion = "Entrada por compra";
        var usuarioId = Guid.NewGuid();

        // Act
        var movimiento = new MovimientoStock(productoId, cantidad, tipo, descripcion, usuarioId);

        // Assert
        movimiento.ProductoId.Should().Be(productoId);
        movimiento.Cantidad.Should().Be(cantidad);
        movimiento.Tipo.Should().Be(tipo);
        movimiento.Descripcion.Should().Be("Entrada por compra");
        movimiento.UsuarioId.Should().Be(usuarioId);
        movimiento.Fecha.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_CantidadCero_DebeLanzarDomainException()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        // Act
        Action act = () => new MovimientoStock(productoId, 0, TipoMovimiento.Ingreso, "Descripción", usuarioId);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("La cantidad del movimiento no puede ser cero.");
    }
}
