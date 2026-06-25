using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using InventorySalesApi.Application.Features.Ventas.Commands;
using InventorySalesApi.Application.Features.Ventas.Handlers;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using InventorySalesApi.Domain.Enums;

namespace InventorySalesApi.UnitTests;

public class CrearVentaTest
{
    private readonly Mock<IVentaRepository> _ventaRepositoryMock;
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IProductoRepository> _productoRepositoryMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CrearVentaCommandHandler _handler;

    public CrearVentaTest()
    {
        _ventaRepositoryMock = new Mock<IVentaRepository>();
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _productoRepositoryMock = new Mock<IProductoRepository>();
        _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CrearVentaCommandHandler(
            _ventaRepositoryMock.Object,
            _clienteRepositoryMock.Object,
            _productoRepositoryMock.Object,
            _usuarioRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_DatosValidos_DebeCrearVentaExitosamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("Juan", "Pérez", new Email("juan@example.com"), "123456", "DNI-1");
        cliente.RegistrarCreacion("admin");
        
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario("vendedor1", new Email("vendedor@example.com"), "hash", RolUsuario.Vendedor);
        usuario.RegistrarCreacion("admin");

        var productoId = Guid.NewGuid();
        var producto = new Producto("PROD-1", "Teclado Mecánico", "RGB", new Money(50.00m), 20, Guid.NewGuid());
        producto.RegistrarCreacion("admin");

        _clienteRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _usuarioRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _productoRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);

        var detalles = new List<CrearDetalleVentaDto>
        {
            new CrearDetalleVentaDto(productoId, 2, 5.00m) // 2 unidades con 5.00 de descuento total
        };
        var command = new CrearVentaCommand(clienteId, detalles, usuarioId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        producto.Stock.Should().Be(18); // 20 - 2 = 18
        _ventaRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Venta>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StockInsuficiente_DebeLanzarDomainException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente("Juan", "Pérez", new Email("juan@example.com"), "123456", "DNI-1");
        
        var usuarioId = Guid.NewGuid();
        var usuario = new Usuario("vendedor1", new Email("vendedor@example.com"), "hash", RolUsuario.Vendedor);

        var productoId = Guid.NewGuid();
        var producto = new Producto("PROD-1", "Teclado Mecánico", "RGB", new Money(50.00m), 1, Guid.NewGuid()); // stock es 1

        _clienteRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);
        _usuarioRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _productoRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);

        var detalles = new List<CrearDetalleVentaDto>
        {
            new CrearDetalleVentaDto(productoId, 5, 0.00m) // solicita 5 unidades
        };
        var command = new CrearVentaCommand(clienteId, detalles, usuarioId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Stock insuficiente*");
        
        _ventaRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Venta>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
