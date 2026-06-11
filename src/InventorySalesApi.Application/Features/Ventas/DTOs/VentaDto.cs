using System;
using System.Collections.Generic;
using System.Linq;
using InventorySalesApi.Domain.Entities;

namespace InventorySalesApi.Application.Features.Ventas.DTOs;

public class VentaDto
{
    public Guid Id { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public Guid ClienteId { get; set; }
    public string ClienteNombreCompleto { get; set; } = null!;
    public decimal Total { get; set; }
    public string Estado { get; set; } = null!;
    public List<DetalleVentaDto> Detalles { get; set; } = new();

    public static VentaDto FromEntity(Venta venta)
    {
        return new VentaDto
        {
            Id = venta.Id,
            Fecha = venta.Fecha,
            ClienteId = venta.ClienteId,
            ClienteNombreCompleto = venta.Cliente != null 
                ? $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}".Trim() 
                : "Cliente Desconocido",
            Total = venta.Total.Amount,
            Estado = venta.Estado.ToString(),
            Detalles = venta.Detalles.Select(d => new DetalleVentaDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? "Producto Desconocido",
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario.Amount,
                Descuento = d.Descuento.Amount,
                Subtotal = d.Subtotal.Amount
            }).ToList()
        };
    }
}
