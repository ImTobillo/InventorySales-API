using System;
using InventorySalesApi.Domain.Entities;

namespace InventorySalesApi.Application.Features.Stock.DTOs;

public class MovimientoStockDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = null!;
    public int Cantidad { get; set; }
    public string Tipo { get; set; } = null!;
    public DateTimeOffset Fecha { get; set; }
    public string Descripcion { get; set; } = null!;
    public Guid UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = null!;

    public static MovimientoStockDto FromEntity(MovimientoStock mov)
    {
        return new MovimientoStockDto
        {
            Id = mov.Id,
            ProductoId = mov.ProductoId,
            ProductoNombre = mov.Producto != null ? mov.Producto.Nombre : "Producto Desconocido",
            Cantidad = mov.Cantidad,
            Tipo = mov.Tipo.ToString(),
            Fecha = mov.Fecha,
            Descripcion = mov.Descripcion,
            UsuarioId = mov.UsuarioId,
            UsuarioNombre = mov.Usuario != null ? mov.Usuario.NombreUsuario : "Usuario Desconocido"
        };
    }
}
