using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Stock.DTOs;
using InventorySalesApi.Application.Features.Stock.Queries;
using InventorySalesApi.Application.Interfaces;
using MediatR;

namespace InventorySalesApi.Application.Features.Stock.Handlers;

public class ObtenerMovimientosQueryHandler : IRequestHandler<ObtenerMovimientosQuery, IReadOnlyList<MovimientoStockDto>>
{
    private readonly IMovimientoStockRepository _movimientoStockRepository;

    public ObtenerMovimientosQueryHandler(IMovimientoStockRepository movimientoStockRepository)
    {
        _movimientoStockRepository = movimientoStockRepository;
    }

    public async Task<IReadOnlyList<MovimientoStockDto>> Handle(ObtenerMovimientosQuery request, CancellationToken cancellationToken)
    {
        var movimientos = request.ProductoId.HasValue
            ? await _movimientoStockRepository.ObtenerPorProductoIdAsync(request.ProductoId.Value, cancellationToken)
            : await _movimientoStockRepository.ObtenerTodosAsync(cancellationToken);

        return movimientos.Select(MovimientoStockDto.FromEntity).ToList().AsReadOnly();
    }
}
