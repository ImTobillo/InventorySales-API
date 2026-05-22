using System;

namespace InventorySalesApi.Domain.Common;

/// <summary>
/// Clase base para todas las entidades del dominio, proporcionando identidad y campos de auditoría.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    public DateTimeOffset FechaCreacion { get; private set; }
    public string CreadoPor { get; private set; } = "System";

    public DateTimeOffset? FechaModificacion { get; private set; }
    public string? ModificadoPor { get; private set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        FechaCreacion = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Registra la información de creación de la entidad.
    /// </summary>
    public void RegistrarCreacion(string usuario)
    {
        FechaCreacion = DateTimeOffset.UtcNow;
        CreadoPor = string.IsNullOrWhiteSpace(usuario) ? "System" : usuario.Trim();
    }

    /// <summary>
    /// Registra la información de modificación de la entidad.
    /// </summary>
    public void RegistrarModificacion(string usuario)
    {
        FechaModificacion = DateTimeOffset.UtcNow;
        ModificadoPor = string.IsNullOrWhiteSpace(usuario) ? "System" : usuario.Trim();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        if (Id == Guid.Empty || other.Id == Guid.Empty)
        {
            return false;
        }

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }

    public static bool operator ==(BaseEntity? a, BaseEntity? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(BaseEntity? a, BaseEntity? b) => !(a == b);
}
