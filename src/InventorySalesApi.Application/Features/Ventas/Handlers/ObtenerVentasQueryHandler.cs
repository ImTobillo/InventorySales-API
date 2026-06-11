using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Ventas.DTOs;
using InventorySalesApi.Application.Features.Ventas.Queries;
using InventorySalesApi.Application.Interfaces;
using MediatR;

namespace InventorySalesApi.Application.Features.Ventas.Handlers;

public class ObtenerVentasQueryHandler : IRequestHandler<ObtenerVentasQuery, IReadOnlyList<VentaDto>>
{
    private readonly IVentaRepository _ventaRepository;

    public ObtenerVentasQueryHandler(IVentaRepository ventaRepository)
    {
        _ventaRepository = ventaRepository;
    }

    public async Task<IReadOnlyList<VentaDto>> Handle(ObtenerVentasQuery request, CancellationToken cancellationToken)
    {
        var ventas = await _ventaRepository.ObtenerTodosAsync(cancellationToken);
        return ventas.Select(VentaDto.FromEntity).ToList().AsReadOnly();
    }
}
