using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Stock.Commands;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Enums;
using InventorySalesApi.Domain.Exceptions;
using MediatR;

namespace InventorySalesApi.Application.Features.Stock.Handlers;

public class RegistrarEntradaStockCommandHandler : IRequestHandler<RegistrarEntradaStockCommand, Unit>
{
    private readonly IProductoRepository _productoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarEntradaStockCommandHandler(
        IProductoRepository productoRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork)
    {
        _productoRepository = productoRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RegistrarEntradaStockCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar producto
        var producto = await _productoRepository.ObtenerPorIdAsync(request.ProductoId, cancellationToken);
        if (producto == null)
        {
            throw new NotFoundException($"El producto con ID {request.ProductoId} no existe.");
        }

        // 2. Validar usuario
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new NotFoundException($"El usuario con ID {request.UsuarioId} no existe.");
        }

        // 3. Registrar movimiento en la entidad Producto (ejecuta validación interna del dominio y actualiza stock)
        producto.RegistrarMovimientoStock(
            request.Cantidad,
            TipoMovimiento.Ingreso,
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
