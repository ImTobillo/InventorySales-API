using System;
using System.Collections.Generic;
using InventorySalesApi.Domain.Common;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using InventorySalesApi.Domain.Enums;

namespace InventorySalesApi.Domain.Entities;

/// <summary>
/// Representa a un usuario del sistema con credenciales y roles.
/// </summary>
public class Usuario : BaseEntity, IAggregateRoot
{
    private readonly List<MovimientoStock> _movimientosStock = new();

    public string NombreUsuario { get; private set; } = null!;
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; }

    // Propiedad de navegación
    public virtual IReadOnlyCollection<MovimientoStock> MovimientosStock => _movimientosStock.AsReadOnly();

    // Constructor privado para EF Core
    private Usuario() : base()
    {
    }

    /// <summary>
    /// Crea un nuevo usuario en el sistema.
    /// </summary>
    public Usuario(string nombreUsuario, Email email, string passwordHash, RolUsuario rol) : base()
    {
        ValidarDatos(nombreUsuario, passwordHash);

        NombreUsuario = nombreUsuario.Trim().ToLower();
        Email = email;
        PasswordHash = passwordHash;
        Rol = rol;
        Activo = true; // Por defecto activo al crearse
    }

    /// <summary>
    /// Actualiza la información del perfil de usuario.
    /// </summary>
    public void ActualizarPerfil(string nombreUsuario, Email email, RolUsuario rol)
    {
        ValidarDatos(nombreUsuario, PasswordHash);

        NombreUsuario = nombreUsuario.Trim().ToLower();
        Email = email;
        Rol = rol;
    }

    /// <summary>
    /// Cambia la contraseña cifrada del usuario.
    /// </summary>
    public void CambiarPassword(string nuevoPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoPasswordHash))
        {
            throw new DomainException("El nuevo hash de contraseña no puede estar vacío.");
        }
        PasswordHash = nuevoPasswordHash;
    }

    /// <summary>
    /// Desactiva al usuario impidiendo su inicio de sesión.
    /// </summary>
    public void Desactivar()
    {
        Activo = false;
    }

    /// <summary>
    /// Activa al usuario.
    /// </summary>
    public void Activar()
    {
        Activo = true;
    }

    private static void ValidarDatos(string nombreUsuario, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
        {
            throw new DomainException("El nombre de usuario es requerido.");
        }

        if (nombreUsuario.Length > 50)
        {
            throw new DomainException("El nombre de usuario no puede superar los 50 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("El hash de contraseña es requerido.");
        }
    }
}
