using System;
using InventorySalesApi.Domain.Entities;

namespace InventorySalesApi.Application.Features.Clientes.DTOs;

public class ClienteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Telefono { get; set; }
    public string DocumentoIdentidad { get; set; } = null!;
    public bool Activo { get; set; }

    public static ClienteDto FromEntity(Cliente cliente)
    {
        return new ClienteDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            Email = cliente.Email.Value,
            Telefono = cliente.Telefono,
            DocumentoIdentidad = cliente.DocumentoIdentidad,
            Activo = cliente.Activo
        };
    }
}
