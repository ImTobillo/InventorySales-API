using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Clientes.DTOs;
using InventorySalesApi.Application.Features.Clientes.Queries;
using InventorySalesApi.Application.Interfaces;
using MediatR;

namespace InventorySalesApi.Application.Features.Clientes.Handlers;

public class ObtenerClientesQueryHandler : IRequestHandler<ObtenerClientesQuery, IReadOnlyList<ClienteDto>>
{
    private readonly IClienteRepository _clienteRepository;

    public ObtenerClientesQueryHandler(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<IReadOnlyList<ClienteDto>> Handle(ObtenerClientesQuery request, CancellationToken cancellationToken)
    {
        var clientes = await _clienteRepository.ObtenerTodosAsync(cancellationToken);
        return clientes.Select(ClienteDto.FromEntity).ToList().AsReadOnly();
    }
}
