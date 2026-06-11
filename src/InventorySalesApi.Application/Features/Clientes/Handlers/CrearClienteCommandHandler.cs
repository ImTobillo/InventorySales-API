using System;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Clientes.Commands;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Domain.ValueObjects;
using MediatR;

namespace InventorySalesApi.Application.Features.Clientes.Handlers;

public class CrearClienteCommandHandler : IRequestHandler<CrearClienteCommand, Guid>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearClienteCommandHandler(IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
    {
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar Email único
        var clienteConEmail = await _clienteRepository.ObtenerPorEmailAsync(request.Email, cancellationToken);
        if (clienteConEmail != null)
        {
            throw new DomainException($"El correo electrónico '{request.Email}' ya está registrado para otro cliente.");
        }

        // 2. Validar DocumentoIdentidad único
        var clienteConDoc = await _clienteRepository.ObtenerPorDocumentoAsync(request.DocumentoIdentidad, cancellationToken);
        if (clienteConDoc != null)
        {
            throw new DomainException($"El documento de identidad '{request.DocumentoIdentidad}' ya está registrado para otro cliente.");
        }

        // 3. Instanciar entidad de Dominio aplicando reglas y value objects
        var email = new Email(request.Email);
        var cliente = new Cliente(
            request.Nombre,
            request.Apellido,
            email,
            request.Telefono,
            request.DocumentoIdentidad
        );

        // 4. Registrar auditoría básica
        cliente.RegistrarCreacion("Sistema");

        // 5. Persistir
        await _clienteRepository.AgregarAsync(cliente, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return cliente.Id;
    }
}
