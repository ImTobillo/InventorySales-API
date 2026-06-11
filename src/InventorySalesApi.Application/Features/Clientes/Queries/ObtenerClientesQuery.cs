using System.Collections.Generic;
using InventorySalesApi.Application.Features.Clientes.DTOs;
using MediatR;

namespace InventorySalesApi.Application.Features.Clientes.Queries;

public record ObtenerClientesQuery() : IRequest<IReadOnlyList<ClienteDto>>;
