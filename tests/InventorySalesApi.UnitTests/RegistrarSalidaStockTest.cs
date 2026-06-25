using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using InventorySalesApi.Application.Features.Stock.Commands;
using InventorySalesApi.Application.Features.Stock.Handlers;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using InventorySalesApi.Domain.Enums;
using MediatR;

namespace InventorySalesApi.UnitTests;

public class RegistrarSalidaStockTest
{
    private readonly Mock<IProductoRepository> _productoRepositoryMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegistrarSalidaStockCommandHandler _handler;

    public RegistrarSalidaStockTest()
    {
        _productoRepositoryMock = new Mock<IProductoRepository>();
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new RegistrarSalidaStockCommandHandler(
            _productoRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_DatosValidos_DebeRegistrarSalidaStockExitosamente()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var producto = new Producto("PROD-1", "Teclado Mecánico", "RGB", new Money(50.00m), 20, Guid.NewGuid());
        producto.RegistrarCreacion("admin");

        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario("operador1", new Email("operador@example.com"), "hash", RolUsuario.Almacenero);
        usuario.RegistrarCreacion("admin");

        _productoRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _usuarioRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new RegistrarSalidaStockCommand(productoId, 5, "Envío a sucursal", usuarioId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        producto.Stock.Should().Be(15); // 20 - 5 = 15
        _productoRepositoryMock.Verify(r => r.Actualizar(producto), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StockInsuficiente_DebeLanzarDomainException()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var producto = new Producto("PROD-1", "Teclado Mecánico", "RGB", new Money(50.00m), 3, Guid.NewGuid()); // stock es 3
        producto.RegistrarCreacion("admin");

        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario("operador1", new Email("operador@example.com"), "hash", RolUsuario.Almacenero);
        usuario.RegistrarCreacion("admin");

        _productoRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _usuarioRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var command = new RegistrarSalidaStockCommand(productoId, 10, "Envío a sucursal", usuarioId); // solicita 10

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Stock insuficiente para el producto 'Teclado Mecánico' (SKU: PROD-1). Disponible: 3, Solicitado: 10.");

        _productoRepositoryMock.Verify(r => r.Actualizar(It.IsAny<Producto>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
