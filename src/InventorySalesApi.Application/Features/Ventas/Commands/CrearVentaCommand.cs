using System;
using System.Collections.Generic;
using MediatR;

namespace InventorySalesApi.Application.Features.Ventas.Commands;

public record CrearDetalleVentaDto(Guid ProductoId, int Cantidad, decimal Descuento);

public record CrearVentaCommand(
    Guid ClienteId,
    List<CrearDetalleVentaDto> Detalles,
    Guid UsuarioId) : IRequest<Guid>;
