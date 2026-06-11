using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Productos.DTOs;
using InventorySalesApi.Application.Features.Productos.Queries;
using InventorySalesApi.Application.Interfaces;
using MediatR;

namespace InventorySalesApi.Application.Features.Productos.Handlers;

public class ObtenerProductosQueryHandler : IRequestHandler<ObtenerProductosQuery, IReadOnlyList<ProductoDto>>
{
    private readonly IProductoRepository _productoRepository;

    public ObtenerProductosQueryHandler(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<IReadOnlyList<ProductoDto>> Handle(ObtenerProductosQuery request, CancellationToken cancellationToken)
    {
        var productos = await _productoRepository.ObtenerTodosAsync(cancellationToken);
        return productos.Select(ProductoDto.FromEntity).ToList().AsReadOnly();
    }
}
