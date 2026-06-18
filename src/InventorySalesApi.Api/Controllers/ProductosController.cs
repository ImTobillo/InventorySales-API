using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using InventorySalesApi.Application.Features.Productos.Commands;
using InventorySalesApi.Application.Features.Productos.Queries;

namespace InventorySalesApi.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Vendedor,Operador")]
    public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
    {
        var query = new ObtenerProductosQuery();
        var resultado = await _mediator.Send(query, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrador,Vendedor,Operador")]
    public async Task<IActionResult> ObtenerPorId([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new ObtenerProductoPorIdQuery(id);
        var resultado = await _mediator.Send(query, cancellationToken);
        if (resultado == null)
        {
            return NotFound($"Producto con ID '{id}' no encontrado.");
        }
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Operador")]
    public async Task<IActionResult> Crear([FromBody] CrearProductoCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return BadRequest("El comando de creación de producto es requerido.");
        }

        var productoId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = productoId }, new { Id = productoId });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Operador")]
    public async Task<IActionResult> Actualizar([FromRoute] Guid id, [FromBody] ActualizarProductoInput input, CancellationToken cancellationToken)
    {
        if (input == null)
        {
            return BadRequest("Los datos de actualización del producto son requeridos.");
        }

        var command = new ActualizarProductoCommand(id, input.Nombre, input.Descripcion, input.Precio, input.CategoriaId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new EliminarProductoCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public record ActualizarProductoInput(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    Guid CategoriaId
);
