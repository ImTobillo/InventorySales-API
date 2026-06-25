using System;
using FluentAssertions;
using Xunit;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;

namespace InventorySalesApi.UnitTests;

public class ClienteDomainTest
{
    [Fact]
    public void Constructor_DatosValidos_DebeCrearClienteCorrectamente()
    {
        // Arrange
        var nombre = "Lucía";
        var apellido = "Fernández";
        var email = new Email("lucia@gmail.com");
        var telefono = "555-4321";
        var documento = "DNI-456789";

        // Act
        var cliente = new Cliente(nombre, apellido, email, telefono, documento);

        // Assert
        cliente.Nombre.Should().Be("Lucía");
        cliente.Apellido.Should().Be("Fernández");
        cliente.Email.Value.Should().Be("lucia@gmail.com");
        cliente.Telefono.Should().Be("555-4321");
        cliente.DocumentoIdentidad.Should().Be("DNI-456789");
        cliente.Activo.Should().BeTrue();
    }

    [Fact]
    public void Constructor_EmailInvalido_DebeLanzarDomainException()
    {
        // Arrange & Act
        Action act = () => new Email("email_invalido");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("El formato del correo electrónico 'email_invalido' es inválido.");
    }

    [Fact]
    public void Desactivar_DebeCambiarEstadoAInactivo()
    {
        // Arrange
        var cliente = new Cliente("Lucía", "Fernández", new Email("lucia@gmail.com"), "1234", "DNI-1");

        // Act
        cliente.Desactivar();

        // Assert
        cliente.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_DebeCambiarEstadoAActivo()
    {
        // Arrange
        var cliente = new Cliente("Lucía", "Fernández", new Email("lucia@gmail.com"), "1234", "DNI-1");
        cliente.Desactivar();

        // Act
        cliente.Activar();

        // Assert
        cliente.Activo.Should().BeTrue();
    }
}
