using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using InventorySalesApi.Application.Features.Stock.Commands;
using InventorySalesApi.Application.Features.Stock.Queries;

namespace InventorySalesApi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public class StockController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("movimientos")]
    [Authorize(Roles = "Administrador,Operador")]
    public async Task<IActionResult> ObtenerMovimientos([FromQuery] Guid? productoId, CancellationToken cancellationToken)
    {
        var query = new ObtenerMovimientosQuery(productoId);
        var resultado = await _mediator.Send(query, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("entrada")]
    [Authorize(Roles = "Administrador,Operador")]
    public async Task<IActionResult> Entrada([FromBody] RegistrarEntradaStockInput input, CancellationToken cancellationToken)
    {
        if (input == null)
        {
            return BadRequest("Los datos de entrada de stock son requeridos.");
        }

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("Usuario no identificado en el token.");
        }

        var command = new RegistrarEntradaStockCommand(input.ProductoId, input.Cantidad, input.Descripcion, userId);
        await _mediator.Send(command, cancellationToken);
        return Ok("Ingreso de stock registrado con éxito.");
    }

    [HttpPost("salida")]
    [Authorize(Roles = "Administrador,Operador")]
    public async Task<IActionResult> Salida([FromBody] RegistrarSalidaStockInput input, CancellationToken cancellationToken)
    {
        if (input == null)
        {
            return BadRequest("Los datos de salida de stock son requeridos.");
        }

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized("Usuario no identificado en el token.");
        }

        var command = new RegistrarSalidaStockCommand(input.ProductoId, input.Cantidad, input.Descripcion, userId);
        await _mediator.Send(command, cancellationToken);
        return Ok("Egreso de stock registrado con éxito.");
    }
}

public record RegistrarEntradaStockInput(
    Guid ProductoId,
    int Cantidad,
    string Descripcion
);

public record RegistrarSalidaStockInput(
    Guid ProductoId,
    int Cantidad,
    string Descripcion
);
