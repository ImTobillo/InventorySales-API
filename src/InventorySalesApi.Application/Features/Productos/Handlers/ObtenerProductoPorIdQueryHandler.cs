using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Productos.DTOs;
using InventorySalesApi.Application.Features.Productos.Queries;
using InventorySalesApi.Application.Interfaces;
using MediatR;

namespace InventorySalesApi.Application.Features.Productos.Handlers;

public class ObtenerProductoPorIdQueryHandler : IRequestHandler<ObtenerProductoPorIdQuery, ProductoDto?>
{
    private readonly IProductoRepository _productoRepository;

    public ObtenerProductoPorIdQueryHandler(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<ProductoDto?> Handle(ObtenerProductoPorIdQuery request, CancellationToken cancellationToken)
    {
        var producto = await _productoRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
        if (producto == null)
        {
            return null;
        }

        return ProductoDto.FromEntity(producto);
    }
}
