using System;
using MediatR;

namespace InventorySalesApi.Application.Features.Productos.Commands;

public record EliminarProductoCommand(Guid Id) : IRequest<Unit>;
