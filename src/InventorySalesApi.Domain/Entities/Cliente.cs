using System;
using System.Collections.Generic;
using InventorySalesApi.Domain.Common;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;

namespace InventorySalesApi.Domain.Entities;

/// <summary>
/// Representa a un cliente del sistema que puede realizar compras.
/// </summary>
public class Cliente : BaseEntity, IAggregateRoot
{
    private readonly List<Venta> _ventas = new();

    public string Nombre { get; private set; } = null!;
    public string Apellido { get; private set; } = null!;
    public Email Email { get; private set; }
    public string? Telefono { get; private set; }
    public string DocumentoIdentidad { get; private set; } = null!;
    public bool Activo { get; private set; }

    // Propiedad de navegación
    public virtual IReadOnlyCollection<Venta> Ventas => _ventas.AsReadOnly();

    // Constructor privado para EF Core
    private Cliente() : base()
    {
    }

    /// <summary>
    /// Crea un nuevo cliente con validaciones de negocio.
    /// </summary>
    public Cliente(string nombre, string apellido, Email email, string? telefono, string documentoIdentidad) : base()
    {
        ValidarDatos(nombre, apellido, documentoIdentidad);

        Nombre = nombre.Trim();
        Apellido = apellido.Trim();
        Email = email;
        Telefono = telefono?.Trim();
        DocumentoIdentidad = documentoIdentidad.Trim().ToUpper();
        Activo = true; // Por defecto activo al crearse
    }

    /// <summary>
    /// Actualiza la información personal y de contacto del cliente.
    /// </summary>
    public void Actualizar(string nombre, string apellido, Email email, string? telefono, string documentoIdentidad)
    {
        ValidarDatos(nombre, apellido, documentoIdentidad);

        Nombre = nombre.Trim();
        Apellido = apellido.Trim();
        Email = email;
        Telefono = telefono?.Trim();
        DocumentoIdentidad = documentoIdentidad.Trim().ToUpper();
    }

    /// <summary>
    /// Desactiva al cliente.
    /// </summary>
    public void Desactivar()
    {
        Activo = false;
    }

    /// <summary>
    /// Activa al cliente.
    /// </summary>
    public void Activar()
    {
        Activo = true;
    }

    private static void ValidarDatos(string nombre, string apellido, string documentoIdentidad)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainException("El nombre del cliente no puede estar vacío.");
        }

        if (nombre.Length > 100)
        {
            throw new DomainException("El nombre del cliente no puede superar los 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(apellido))
        {
            throw new DomainException("El apellido del cliente no puede estar vacío.");
        }

        if (apellido.Length > 100)
        {
            throw new DomainException("El apellido del cliente no puede superar los 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(documentoIdentidad))
        {
            throw new DomainException("El documento de identidad del cliente es requerido.");
        }

        if (documentoIdentidad.Length > 30)
        {
            throw new DomainException("El documento de identidad no puede superar los 30 caracteres.");
        }
    }
}
