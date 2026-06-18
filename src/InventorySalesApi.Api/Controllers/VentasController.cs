using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using InventorySalesApi.Application.Features.Ventas.Commands;
using InventorySalesApi.Application.Features.Ventas.Queries;

namespace InventorySalesApi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public class VentasController : ControllerBase
{
    private readonly IMediator _mediator;

    public VentasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
    {
        var query = new ObtenerVentasQuery();
        var resultado = await _mediator.Send(query, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> ObtenerPorId([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new ObtenerVentaPorIdQuery(id);
        var resultado = await _mediator.Send(query, cancellationToken);
        if (resultado == null)
        {
            return NotFound($"Venta con ID '{id}' no encontrada.");
        }
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Crear([FromBody] CrearVentaInput input, CancellationToken cancellationToken)
    {
        if (input == null)
        {
            return BadRequest("Los detalles de la venta son requeridos.");
        }

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("Usuario no identificado en el token.");
        }

        var command = new CrearVentaCommand(input.ClienteId, input.Detalles, userId);
        var ventaId = await _mediator.Send(command, cancellationToken);
        
        return CreatedAtAction(nameof(ObtenerPorId), new { id = ventaId }, new { Id = ventaId });
    }

    [HttpPut("{id:guid}/cancelar")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Cancelar([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("Usuario no identificado en el token.");
        }

        var command = new CancelarVentaCommand(id, userId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public record CrearVentaInput(
    Guid ClienteId,
    List<CrearDetalleVentaDto> Detalles
);
