using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.Usuarios.ToListAsync(cancellationToken);
        return list.AsReadOnly();
    }

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
    }
}
