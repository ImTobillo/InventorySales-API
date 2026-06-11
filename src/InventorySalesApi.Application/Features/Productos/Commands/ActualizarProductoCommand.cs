using System;
using MediatR;

namespace InventorySalesApi.Application.Features.Productos.Commands;

public record ActualizarProductoCommand(
    Guid Id,
    string Nombre,
    string? Descripcion,
    decimal Precio,
    Guid CategoriaId) : IRequest<Unit>;
