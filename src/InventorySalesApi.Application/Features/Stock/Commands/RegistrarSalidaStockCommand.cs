using System;
using MediatR;

namespace InventorySalesApi.Application.Features.Stock.Commands;

public record RegistrarSalidaStockCommand(
    Guid ProductoId,
    int Cantidad,
    string Descripcion,
    Guid UsuarioId) : IRequest<Unit>;
