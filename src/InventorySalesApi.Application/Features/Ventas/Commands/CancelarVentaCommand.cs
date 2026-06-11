using System;
using MediatR;

namespace InventorySalesApi.Application.Features.Ventas.Commands;

public record CancelarVentaCommand(Guid VentaId, Guid UsuarioId) : IRequest<Unit>;
