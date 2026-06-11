using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Ventas.DTOs;
using InventorySalesApi.Application.Features.Ventas.Queries;
using InventorySalesApi.Application.Interfaces;
using MediatR;

namespace InventorySalesApi.Application.Features.Ventas.Handlers;

public class ObtenerVentaPorIdQueryHandler : IRequestHandler<ObtenerVentaPorIdQuery, VentaDto?>
{
    private readonly IVentaRepository _ventaRepository;

    public ObtenerVentaPorIdQueryHandler(IVentaRepository ventaRepository)
    {
        _ventaRepository = ventaRepository;
    }

    public async Task<VentaDto?> Handle(ObtenerVentaPorIdQuery request, CancellationToken cancellationToken)
    {
        var venta = await _ventaRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
        if (venta == null)
        {
            return null;
        }

        return VentaDto.FromEntity(venta);
    }
}
