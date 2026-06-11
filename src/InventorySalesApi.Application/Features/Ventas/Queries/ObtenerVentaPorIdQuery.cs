using System;
using InventorySalesApi.Application.Features.Ventas.DTOs;
using MediatR;

namespace InventorySalesApi.Application.Features.Ventas.Queries;

public record ObtenerVentaPorIdQuery(Guid Id) : IRequest<VentaDto?>;
