using System.Collections.Generic;
using InventorySalesApi.Domain.Common;
using InventorySalesApi.Domain.Exceptions;

namespace InventorySalesApi.Domain.Entities;

/// <summary>
/// Representa una categoría de productos en el inventario.
/// </summary>
public class Categoria : BaseEntity, IAggregateRoot
{
    private readonly List<Producto> _productos = new();

    public string Nombre { get; private set; } = null!;
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; }

    // Propiedad de navegación encapsulada
    public virtual IReadOnlyCollection<Producto> Productos => _productos.AsReadOnly();

    // Constructor privado requerido por EF Core / ORMs
    private Categoria() : base()
    {
    }

    /// <summary>
    /// Crea una nueva categoría con validaciones iniciales de dominio.
    /// </summary>
    public Categoria(string nombre, string? descripcion) : base()
    {
        ValidarNombre(nombre);
        
        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
        Activo = true; // Por defecto toda nueva categoría se crea activa
    }

    /// <summary>
    /// Actualiza los datos de la categoría aplicando validaciones.
    /// </summary>
    public void Actualizar(string nombre, string? descripcion)
    {
        ValidarNombre(nombre);

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
    }

    /// <summary>
    /// Desactiva la categoría.
    /// </summary>
    public void Desactivar()
    {
        Activo = false;
    }

    /// <summary>
    /// Activa la categoría.
    /// </summary>
    public void Activar()
    {
        Activo = true;
    }

    private static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new DomainException("El nombre de la categoría no puede estar vacío.");
        }

        if (nombre.Length > 100)
        {
            throw new DomainException("El nombre de la categoría no puede superar los 100 caracteres.");
        }
    }
}
