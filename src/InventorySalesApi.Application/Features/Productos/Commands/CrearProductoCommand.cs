using System;
using MediatR;

namespace InventorySalesApi.Application.Features.Productos.Commands;

public record CrearProductoCommand(
    string Sku,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int StockInicial,
    Guid CategoriaId) : IRequest<Guid>;
