using System;
using InventorySalesApi.Application.Features.Productos.DTOs;
using MediatR;

namespace InventorySalesApi.Application.Features.Productos.Queries;

public record ObtenerProductoPorIdQuery(Guid Id) : IRequest<ProductoDto?>;
