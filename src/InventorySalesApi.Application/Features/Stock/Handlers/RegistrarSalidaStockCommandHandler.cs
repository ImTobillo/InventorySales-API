using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Stock.Commands;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Enums;
using InventorySalesApi.Domain.Exceptions;
using MediatR;

namespace InventorySalesApi.Application.Features.Stock.Handlers;

public class RegistrarSalidaStockCommandHandler : IRequestHandler<RegistrarSalidaStockCommand, Unit>
{
    private readonly IProductoRepository _productoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarSalidaStockCommandHandler(
        IProductoRepository productoRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork)
    {
        _productoRepository = productoRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RegistrarSalidaStockCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar producto
        var producto = await _productoRepository.ObtenerPorIdAsync(request.ProductoId, cancellationToken);
        if (producto == null)
        {
            throw new DomainException($"El producto con ID {request.ProductoId} no existe.");
        }

        // 2. Validar usuario
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new DomainException($"El usuario con ID {request.UsuarioId} no existe.");
        }

        // 3. Registrar salida en la entidad (ejecuta validación interna del dominio de disponibilidad y actualiza stock)
        producto.RegistrarMovimientoStock(
            request.Cantidad,
            TipoMovimiento.Egreso,
            request.Descripcion,
            request.UsuarioId
        );

        // 4. Registrar auditoría básica
        producto.RegistrarModificacion(usuario.NombreUsuario);

        // 5. Persistir
        _productoRepository.Actualizar(producto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
