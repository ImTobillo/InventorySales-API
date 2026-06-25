using System;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Productos.Commands;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using MediatR;

namespace InventorySalesApi.Application.Features.Productos.Handlers;

public class ActualizarProductoCommandHandler : IRequestHandler<ActualizarProductoCommand, Unit>
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarProductoCommandHandler(
        IProductoRepository productoRepository,
        ICategoriaRepository categoriaRepository,
        IUnitOfWork unitOfWork)
    {
        _productoRepository = productoRepository;
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ActualizarProductoCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar existencia del producto
        var producto = await _productoRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
        if (producto == null)
        {
            throw new NotFoundException($"El producto con ID {request.Id} no existe.");
        }

        // 2. Validar existencia de la categoría nueva
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(request.CategoriaId, cancellationToken);
        if (categoria == null)
        {
            throw new NotFoundException($"La categoría especificada con ID {request.CategoriaId} no existe.");
        }

        // 3. Ejecutar actualización sobre el modelo de dominio
        var precio = new Money(request.Precio);
        producto.Actualizar(request.Nombre, request.Descripcion, precio, request.CategoriaId);

        // 4. Registrar auditoría básica de modificación
        producto.RegistrarModificacion("Sistema");

        // 5. Persistir
        _productoRepository.Actualizar(producto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
