using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
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
    private readonly IProductoRepository _productoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegistrarEntradaStockCommandHandler _handler;

    public RegistrarEntradaStockTest()
    {
        _productoRepository = Substitute.For<IProductoRepository>();
        _usuarioRepository = Substitute.For<IUsuarioRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RegistrarEntradaStockCommandHandler(_productoRepository, _usuarioRepository, _unitOfWork);
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

        _productoRepository.ObtenerPorIdAsync(productoId, Arg.Any<CancellationToken>()).Returns(producto);
        _usuarioRepository.ObtenerPorIdAsync(usuarioId, Arg.Any<CancellationToken>()).Returns(usuario);

        var command = new RegistrarEntradaStockCommand(productoId, 5, "Compra a proveedor", usuarioId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        producto.Stock.Should().Be(25); // 20 + 5 = 25
        _productoRepository.Received(1).Actualizar(producto);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProductoNoExiste_DebeLanzarDomainException()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        _productoRepository.ObtenerPorIdAsync(productoId, Arg.Any<CancellationToken>()).Returns((Producto)null!);

        var command = new RegistrarEntradaStockCommand(productoId, 5, "Compra a proveedor", usuarioId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"El producto con ID {productoId} no existe.");

        _productoRepository.DidNotReceive().Actualizar(Arg.Any<Producto>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
