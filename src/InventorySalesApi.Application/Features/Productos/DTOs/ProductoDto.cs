using System;
using InventorySalesApi.Domain.Entities;

namespace InventorySalesApi.Application.Features.Productos.DTOs;

public class ProductoDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = null!;

    public static ProductoDto FromEntity(Producto producto)
    {
        return new ProductoDto
        {
            Id = producto.Id,
            Sku = producto.Sku,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            Precio = producto.Precio.Amount,
            Stock = producto.Stock,
            CategoriaId = producto.CategoriaId,
            CategoriaNombre = producto.Categoria?.Nombre ?? "Sin Categoría"
        };
    }
}
