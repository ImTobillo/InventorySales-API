using System.Threading;
using System.Threading.Tasks;
using InventorySalesApi.Application.Features.Clientes.DTOs;
using InventorySalesApi.Application.Features.Clientes.Queries;
using InventorySalesApi.Application.Interfaces;
using MediatR;

namespace InventorySalesApi.Application.Features.Clientes.Handlers;

public class ObtenerClientePorIdQueryHandler : IRequestHandler<ObtenerClientePorIdQuery, ClienteDto?>
{
    private readonly IClienteRepository _clienteRepository;

    public ObtenerClientePorIdQueryHandler(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<ClienteDto?> Handle(ObtenerClientePorIdQuery request, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
        if (cliente == null)
        {
            return null;
        }

        return ClienteDto.FromEntity(cliente);
    }
}
