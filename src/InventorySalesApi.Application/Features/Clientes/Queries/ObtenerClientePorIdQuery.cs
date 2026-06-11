using System;
using InventorySalesApi.Application.Features.Clientes.DTOs;
using MediatR;

namespace InventorySalesApi.Application.Features.Clientes.Queries;

public record ObtenerClientePorIdQuery(Guid Id) : IRequest<ClienteDto?>;
