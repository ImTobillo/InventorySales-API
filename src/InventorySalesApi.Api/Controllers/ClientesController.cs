using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using InventorySalesApi.Application.Features.Clientes.Commands;
using InventorySalesApi.Application.Features.Clientes.Queries;

namespace InventorySalesApi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
    {
        var query = new ObtenerClientesQuery();
        var resultado = await _mediator.Send(query, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> ObtenerPorId([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new ObtenerClientePorIdQuery(id);
        var resultado = await _mediator.Send(query, cancellationToken);
        if (resultado == null)
        {
            return NotFound($"Cliente con ID '{id}' no encontrado.");
        }
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Crear([FromBody] CrearClienteCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return BadRequest("El comando de creación de cliente es requerido.");
        }

        var clienteId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = clienteId }, new { Id = clienteId });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Actualizar([FromRoute] Guid id, [FromBody] ActualizarClienteInput input, CancellationToken cancellationToken)
    {
        if (input == null)
        {
            return BadRequest("Los datos de actualización del cliente son requeridos.");
        }

        var command = new ActualizarClienteCommand(
            id,
            input.Nombre,
            input.Apellido,
            input.Email,
            input.Telefono,
            input.DocumentoIdentidad
        );
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public record ActualizarClienteInput(
    string Nombre,
    string Apellido,
    string Email,
    string? Telefono,
    string DocumentoIdentidad
);
