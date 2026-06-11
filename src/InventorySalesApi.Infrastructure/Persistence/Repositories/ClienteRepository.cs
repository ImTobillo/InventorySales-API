using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.ValueObjects;

namespace Infrastructure.Persistence.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Cliente>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.Clientes.ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public async Task<Cliente?> ObtenerPorDocumentoAsync(string documentoIdentidad, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentoIdentidad)) return null;
        var docUpper = documentoIdentidad.Trim().ToUpper();
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.DocumentoIdentidad == docUpper, cancellationToken);
    }

    public async Task<Cliente?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var emailLower = email.Trim().ToLower();
        var emailVo = new Email(emailLower);
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Email == emailVo, cancellationToken);
    }

    public async Task AgregarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await _context.Clientes.AddAsync(cliente, cancellationToken);
    }

    public void Actualizar(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
    }
}
