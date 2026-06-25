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

public class RegistrarEntradaStockTest
{
    private readonly Mock<IProductoRepository> _productoRepositoryMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegistrarEntradaStockCommandHandler _handler;

    public RegistrarEntradaStockTest()
    {
        _productoRepositoryMock = new Mock<IProductoRepository>();
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new RegistrarEntradaStockCommandHandler(
            _productoRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_DatosValidos_DebeRegistrarEntradaStockExitosamente()
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

        var command = new RegistrarEntradaStockCommand(productoId, 5, "Compra a proveedor", usuarioId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        producto.Stock.Should().Be(25); // 20 + 5 = 25
        _productoRepositoryMock.Verify(r => r.Actualizar(producto), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductoNoExiste_DebeLanzarNotFoundException()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        _productoRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Producto)null!);

        var command = new RegistrarEntradaStockCommand(productoId, 5, "Compra a proveedor", usuarioId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"El producto con ID {productoId} no existe.");

        _productoRepositoryMock.Verify(r => r.Actualizar(It.IsAny<Producto>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
