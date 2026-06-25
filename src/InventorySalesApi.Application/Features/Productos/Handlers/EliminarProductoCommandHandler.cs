using System;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Productos.Commands;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Exceptions;
using MediatR;

namespace InventorySalesApi.Application.Features.Productos.Handlers;

public class EliminarProductoCommandHandler : IRequestHandler<EliminarProductoCommand, Unit>
{
    private readonly IProductoRepository _productoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarProductoCommandHandler(IProductoRepository productoRepository, IUnitOfWork unitOfWork)
    {
        _productoRepository = productoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(EliminarProductoCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar existencia del producto
        var producto = await _productoRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
        if (producto == null)
        {
            throw new NotFoundException($"El producto con ID {request.Id} no existe y no puede ser eliminado.");
        }

        // 2. Eliminar de la persistencia
        _productoRepository.Eliminar(producto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
