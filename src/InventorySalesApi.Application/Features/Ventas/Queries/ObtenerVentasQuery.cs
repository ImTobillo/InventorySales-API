using System.Collections.Generic;
using InventorySalesApi.Application.Features.Ventas.DTOs;
using MediatR;

namespace InventorySalesApi.Application.Features.Ventas.Queries;

public record ObtenerVentasQuery() : IRequest<IReadOnlyList<VentaDto>>;
