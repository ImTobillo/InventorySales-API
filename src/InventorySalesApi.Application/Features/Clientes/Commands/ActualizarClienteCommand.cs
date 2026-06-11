using System;
using MediatR;

namespace InventorySalesApi.Application.Features.Clientes.Commands;

public record ActualizarClienteCommand(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email,
    string? Telefono,
    string DocumentoIdentidad) : IRequest<Unit>;
