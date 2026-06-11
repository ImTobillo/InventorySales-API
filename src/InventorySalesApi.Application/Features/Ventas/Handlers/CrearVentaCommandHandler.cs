using System;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Ventas.Commands;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;
using MediatR;

namespace InventorySalesApi.Application.Features.Ventas.Handlers;

public class CrearVentaCommandHandler : IRequestHandler<CrearVentaCommand, Guid>
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearVentaCommandHandler(
        IVentaRepository ventaRepository,
        IClienteRepository clienteRepository,
        IProductoRepository productoRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork)
    {
        _ventaRepository = ventaRepository;
        _clienteRepository = clienteRepository;
        _productoRepository = productoRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CrearVentaCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar Cliente
        var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);
        if (cliente == null)
        {
            throw new DomainException($"El cliente con ID {request.ClienteId} no existe.");
        }
        if (!cliente.Activo)
        {
            throw new DomainException($"El cliente '{cliente.Nombre} {cliente.Apellido}' no está activo.");
        }

        // 2. Validar Usuario (Vendedor)
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new DomainException($"El usuario vendedor con ID {request.UsuarioId} no existe.");
        }
        if (!usuario.Activo)
        {
            throw new DomainException($"El usuario '{usuario.NombreUsuario}' no está activo.");
        }

        // 3. Crear cabecera de la Venta
        var venta = new Venta(request.ClienteId);
        venta.RegistrarCreacion(usuario.NombreUsuario);

        // 4. Agregar ítems (cada ítem valida y descuenta stock a través del modelo de dominio)
        foreach (var item in request.Detalles)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
            if (producto == null)
            {
                throw new DomainException($"El producto con ID {item.ProductoId} no existe.");
            }

            venta.AgregarDetalle(producto, item.Cantidad, item.Descuento, request.UsuarioId);
        }

        // 5. Completar la Venta (cambiar estado a Completada)
        venta.CompletarVenta();

        // 6. Registrar en persistencia
        await _ventaRepository.AgregarAsync(venta, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return venta.Id;
    }
}
