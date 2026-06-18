using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Xunit;
using InventorySalesApi.Application.Features.Productos.Commands;
using InventorySalesApi.Application.Features.Productos.Handlers;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;

namespace InventorySalesApi.UnitTests;

public class CrearProductoTest
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CrearProductoCommandHandler _handler;

    public CrearProductoTest()
    {
        _productoRepository = Substitute.For<IProductoRepository>();
        _categoriaRepository = Substitute.For<ICategoriaRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CrearProductoCommandHandler(_productoRepository, _categoriaRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_DatosValidos_DebeCrearProductoExitosamente()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categoria = new Categoria("Electrodomésticos", "Descripción electro");
        
        _categoriaRepository.ObtenerPorIdAsync(categoriaId, Arg.Any<CancellationToken>())
            .Returns(categoria);
            
        _productoRepository.ObtenerPorSkuAsync("NOTE-ASUS-UX340", Arg.Any<CancellationToken>())
            .Returns((Producto)null!);

        var command = new CrearProductoCommand(
            Sku: "NOTE-ASUS-UX340",
            Nombre: "Notebook ASUS ZenBook 14",
            Descripcion: "Intel Core i7, 16GB RAM, 512GB SSD",
            Precio: 1200.50m,
            StockInicial: 10,
            CategoriaId: categoriaId
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _productoRepository.Received(1).AgregarAsync(Arg.Is<Producto>(p => p.Sku == command.Sku && p.Nombre == command.Nombre), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CategoriaNoExiste_DebeLanzarDomainException()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _categoriaRepository.ObtenerPorIdAsync(categoriaId, Arg.Any<CancellationToken>())
            .Returns((Categoria)null!);

        var command = new CrearProductoCommand(
            Sku: "NOTE-ASUS-UX340",
            Nombre: "Notebook ASUS ZenBook 14",
            Descripcion: "Intel Core i7, 16GB RAM, 512GB SSD",
            Precio: 1200.50m,
            StockInicial: 10,
            CategoriaId: categoriaId
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"La categoría especificada con ID {categoriaId} no existe.");
            
        await _productoRepository.DidNotReceive().AgregarAsync(Arg.Any<Producto>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SkuDuplicado_DebeLanzarDomainException()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categoria = new Categoria("Electrodomésticos", "Descripción");
        var productoExistente = new Producto("NOTE-ASUS-UX340", "Notebook ASUS", "Desc", new Domain.ValueObjects.Money(100), 5, categoriaId);

        _categoriaRepository.ObtenerPorIdAsync(categoriaId, Arg.Any<CancellationToken>())
            .Returns(categoria);
            
        _productoRepository.ObtenerPorSkuAsync("NOTE-ASUS-UX340", Arg.Any<CancellationToken>())
            .Returns(productoExistente);

        var command = new CrearProductoCommand(
            Sku: "NOTE-ASUS-UX340",
            Nombre: "Notebook ASUS ZenBook 14",
            Descripcion: "Intel Core i7, 16GB RAM, 512GB SSD",
            Precio: 1200.50m,
            StockInicial: 10,
            CategoriaId: categoriaId
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("El SKU 'NOTE-ASUS-UX340' ya está registrado para otro producto.");
            
        await _productoRepository.DidNotReceive().AgregarAsync(Arg.Any<Producto>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
