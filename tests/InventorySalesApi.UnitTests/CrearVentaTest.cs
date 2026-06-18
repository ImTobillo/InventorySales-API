using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
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
    private readonly IVentaRepository _ventaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrearVentaCommandHandler _handler;

    public CrearVentaTest()
    {
        _ventaRepository = Substitute.For<IVentaRepository>();
        _clienteRepository = Substitute.For<IClienteRepository>();
        _productoRepository = Substitute.For<IProductoRepository>();
        _usuarioRepository = Substitute.For<IUsuarioRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CrearVentaCommandHandler(
            _ventaRepository,
            _clienteRepository,
            _productoRepository,
            _usuarioRepository,
            _unitOfWork
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

        _clienteRepository.ObtenerPorIdAsync(clienteId, Arg.Any<CancellationToken>()).Returns(cliente);
        _usuarioRepository.ObtenerPorIdAsync(usuarioId, Arg.Any<CancellationToken>()).Returns(usuario);
        _productoRepository.ObtenerPorIdAsync(productoId, Arg.Any<CancellationToken>()).Returns(producto);

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
        await _ventaRepository.Received(1).AgregarAsync(Arg.Any<Venta>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
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

        _clienteRepository.ObtenerPorIdAsync(clienteId, Arg.Any<CancellationToken>()).Returns(cliente);
        _usuarioRepository.ObtenerPorIdAsync(usuarioId, Arg.Any<CancellationToken>()).Returns(usuario);
        _productoRepository.ObtenerPorIdAsync(productoId, Arg.Any<CancellationToken>()).Returns(producto);

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
        
        await _ventaRepository.DidNotReceive().AgregarAsync(Arg.Any<Venta>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
