using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using InventorySalesApi.Application.Features.Clientes.Commands;
using InventorySalesApi.Application.Features.Clientes.Handlers;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;

namespace InventorySalesApi.UnitTests;

public class CrearClienteTest
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CrearClienteCommandHandler _handler;

    public CrearClienteTest()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CrearClienteCommandHandler(
            _clienteRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_DatosValidos_DebeCrearClienteExitosamente()
    {
        // Arrange
        _clienteRepositoryMock
            .Setup(r => r.ObtenerPorEmailAsync("cliente@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!);

        _clienteRepositoryMock
            .Setup(r => r.ObtenerPorDocumentoAsync("DNI-123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!);

        var command = new CrearClienteCommand(
            Nombre: "Carlos",
            Apellido: "Gómez",
            Email: "cliente@example.com",
            Telefono: "987654321",
            DocumentoIdentidad: "DNI-123456"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _clienteRepositoryMock.Verify(
            r => r.AgregarAsync(It.Is<Cliente>(c => c.Nombre == command.Nombre && c.Email.Value == command.Email), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmailDuplicado_DebeLanzarDomainException()
    {
        // Arrange
        var clienteExistente = new Cliente("Juan", "Pérez", new Domain.ValueObjects.Email("cliente@example.com"), "1234", "DNI-999");
        _clienteRepositoryMock
            .Setup(r => r.ObtenerPorEmailAsync("cliente@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        var command = new CrearClienteCommand(
            Nombre: "Carlos",
            Apellido: "Gómez",
            Email: "cliente@example.com",
            Telefono: "987654321",
            DocumentoIdentidad: "DNI-123456"
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("El correo electrónico 'cliente@example.com' ya está registrado para otro cliente.");

        _clienteRepositoryMock.Verify(
            r => r.AgregarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DocumentoDuplicado_DebeLanzarDomainException()
    {
        // Arrange
        var clienteExistente = new Cliente("Juan", "Pérez", new Domain.ValueObjects.Email("otro@example.com"), "1234", "DNI-123456");
        _clienteRepositoryMock
            .Setup(r => r.ObtenerPorEmailAsync("cliente@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!);
        _clienteRepositoryMock
            .Setup(r => r.ObtenerPorDocumentoAsync("DNI-123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteExistente);

        var command = new CrearClienteCommand(
            Nombre: "Carlos",
            Apellido: "Gómez",
            Email: "cliente@example.com",
            Telefono: "987654321",
            DocumentoIdentidad: "DNI-123456"
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("El documento de identidad 'DNI-123456' ya está registrado para otro cliente.");

        _clienteRepositoryMock.Verify(
            r => r.AgregarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
