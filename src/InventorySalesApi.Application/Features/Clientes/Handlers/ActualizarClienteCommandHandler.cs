using System;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Clientes.Commands;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using MediatR;

namespace InventorySalesApi.Application.Features.Clientes.Handlers;

public class ActualizarClienteCommandHandler : IRequestHandler<ActualizarClienteCommand, Unit>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarClienteCommandHandler(IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
    {
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ActualizarClienteCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar existencia del cliente
        var cliente = await _clienteRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
        if (cliente == null)
        {
            throw new NotFoundException($"El cliente con ID {request.Id} no existe.");
        }

        // 2. Validar Email único (excluyendo a sí mismo)
        var clienteConEmail = await _clienteRepository.ObtenerPorEmailAsync(request.Email, cancellationToken);
        if (clienteConEmail != null && clienteConEmail.Id != request.Id)
        {
            throw new DomainException($"El correo electrónico '{request.Email}' ya está registrado por otro cliente.");
        }

        // 3. Validar DocumentoIdentidad único (excluyendo a sí mismo)
        var clienteConDoc = await _clienteRepository.ObtenerPorDocumentoAsync(request.DocumentoIdentidad, cancellationToken);
        if (clienteConDoc != null && clienteConDoc.Id != request.Id)
        {
            throw new DomainException($"El documento de identidad '{request.DocumentoIdentidad}' ya está registrado por otro cliente.");
        }

        // 4. Actualizar entidad de dominio
        var email = new Email(request.Email);
        cliente.Actualizar(
            request.Nombre,
            request.Apellido,
            email,
            request.Telefono,
            request.DocumentoIdentidad
        );

        // 5. Registrar auditoría básica
        cliente.RegistrarModificacion("Sistema");

        // 6. Persistir
        _clienteRepository.Actualizar(cliente);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
