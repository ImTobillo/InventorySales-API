using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using InventorySalesApi.Application.Features.Productos.Commands;
using InventorySalesApi.Application.Features.Productos.Handlers;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;

namespace InventorySalesApi.UnitTests;

public class CrearProductoTest
{
    private readonly Mock<IProductoRepository> _productoRepositoryMock;
    private readonly Mock<ICategoriaRepository> _categoriaRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CrearProductoCommandHandler _handler;

    public CrearProductoTest()
    {
        _productoRepositoryMock = new Mock<IProductoRepository>();
        _categoriaRepositoryMock = new Mock<ICategoriaRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CrearProductoCommandHandler(
            _productoRepositoryMock.Object, 
            _categoriaRepositoryMock.Object, 
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_DatosValidos_DebeCrearProductoExitosamente()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categoria = new Categoria("Electrodomésticos", "Descripción electro");
        
        _categoriaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);
            
        _productoRepositoryMock
            .Setup(r => r.ObtenerPorSkuAsync("NOTE-ASUS-UX340", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Producto)null!);

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
        _productoRepositoryMock.Verify(
            r => r.AgregarAsync(It.Is<Producto>(p => p.Sku == command.Sku && p.Nombre == command.Nombre), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoriaNoExiste_DebeLanzarNotFoundException()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        _categoriaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Categoria)null!);

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
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"La categoría especificada con ID {categoriaId} no existe.");
            
        _productoRepositoryMock.Verify(
            r => r.AgregarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SkuDuplicado_DebeLanzarDomainException()
    {
        // Arrange
        var categoriaId = Guid.NewGuid();
        var categoria = new Categoria("Electrodomésticos", "Descripción");
        var productoExistente = new Producto("NOTE-ASUS-UX340", "Notebook ASUS", "Desc", new Domain.ValueObjects.Money(100), 5, categoriaId);

        _categoriaRepositoryMock
            .Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);
            
        _productoRepositoryMock
            .Setup(r => r.ObtenerPorSkuAsync("NOTE-ASUS-UX340", It.IsAny<CancellationToken>()))
            .ReturnsAsync(productoExistente);

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
            
        _productoRepositoryMock.Verify(
            r => r.AgregarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
