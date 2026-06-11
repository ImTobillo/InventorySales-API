using System;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Ventas.Commands;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Exceptions;
using MediatR;

namespace InventorySalesApi.Application.Features.Ventas.Handlers;

public class CancelarVentaCommandHandler : IRequestHandler<CancelarVentaCommand, Unit>
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelarVentaCommandHandler(
        IVentaRepository ventaRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork)
    {
        _ventaRepository = ventaRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CancelarVentaCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar venta existente
        var venta = await _ventaRepository.ObtenerPorIdAsync(request.VentaId, cancellationToken);
        if (venta == null)
        {
            throw new DomainException($"La venta con ID {request.VentaId} no existe.");
        }

        // 2. Validar usuario cancelador
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new DomainException($"El usuario con ID {request.UsuarioId} no existe.");
        }

        // 3. Ejecutar cancelación en el modelo de dominio (devolución automática de stock)
        venta.CancelarVenta(request.UsuarioId);

        // 4. Registrar modificación en la auditoría
        venta.RegistrarModificacion(usuario.NombreUsuario);

        // 5. Persistir
        _ventaRepository.Actualizar(venta);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
