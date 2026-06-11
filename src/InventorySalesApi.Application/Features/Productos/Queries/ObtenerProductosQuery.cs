using System.Collections.Generic;
using InventorySalesApi.Application.Features.Productos.DTOs;
using MediatR;

namespace InventorySalesApi.Application.Features.Productos.Queries;

public record ObtenerProductosQuery() : IRequest<IReadOnlyList<ProductoDto>>;
