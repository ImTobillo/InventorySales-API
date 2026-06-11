using System;
using System.Collections.Generic;
using InventorySalesApi.Application.Features.Stock.DTOs;
using MediatR;

namespace InventorySalesApi.Application.Features.Stock.Queries;

public record ObtenerMovimientosQuery(Guid? ProductoId = null) : IRequest<IReadOnlyList<MovimientoStockDto>>;
