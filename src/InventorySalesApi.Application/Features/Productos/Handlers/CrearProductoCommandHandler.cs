using System;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Productos.Commands;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using MediatR;

namespace InventorySalesApi.Application.Features.Productos.Handlers;

public class CrearProductoCommandHandler : IRequestHandler<CrearProductoCommand, Guid>
{
    private readonly IProductoRepository _productoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearProductoCommandHandler(
        IProductoRepository productoRepository,
        ICategoriaRepository categoriaRepository,
        IUnitOfWork unitOfWork)
    {
        _productoRepository = productoRepository;
        _categoriaRepository = categoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CrearProductoCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar que exista la categoría
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(request.CategoriaId, cancellationToken);
        if (categoria == null)
        {
            throw new DomainException($"La categoría especificada con ID {request.CategoriaId} no existe.");
        }

        // 2. Validar SKU único
        var skuExistente = await _productoRepository.ObtenerPorSkuAsync(request.Sku, cancellationToken);
        if (skuExistente != null)
        {
            throw new DomainException($"El SKU '{request.Sku}' ya está registrado para otro producto.");
        }

        // 3. Crear entidad de Dominio (las reglas de negocio de formato y valores se ejecutan en el constructor)
        var precio = new Money(request.Precio);
        var producto = new Producto(
            request.Sku,
            request.Nombre,
            request.Descripcion,
            precio,
            request.StockInicial,
            request.CategoriaId
        );

        // 4. Registrar auditoría básica
        producto.RegistrarCreacion("Sistema");

        // 5. Persistir
        await _productoRepository.AgregarAsync(producto, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return producto.Id;
    }
}
