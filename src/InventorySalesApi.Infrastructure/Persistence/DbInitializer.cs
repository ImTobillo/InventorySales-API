using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.Enums;
using InventorySalesApi.Domain.ValueObjects;

namespace Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        // Asegurar que la base de datos esté creada
        context.Database.EnsureCreated();

        // Sembrar Categorías si están vacías
        if (!context.Categorias.Any())
        {
            var catElectro = new Categoria("Electrodomésticos", "Dispositivos electrónicos del hogar");
            catElectro.RegistrarCreacion("Semilla");
            await context.Categorias.AddAsync(catElectro);

            var catAlimentos = new Categoria("Alimentos", "Productos alimenticios y consumibles");
            catAlimentos.RegistrarCreacion("Semilla");
            await context.Categorias.AddAsync(catAlimentos);

            await context.SaveChangesAsync();
        }

        // Sembrar Usuarios si están vacíos
        if (!context.Usuarios.Any())
        {
            // Seed Admin User
            var adminUser = new Usuario(
                "admin", 
                new Email("admin@sistema.com"), 
                passwordHasher.HashPassword("AdminPassword123!"), 
                RolUsuario.Administrador
            );
            adminUser.RegistrarCreacion("Semilla");
            await context.Usuarios.AddAsync(adminUser);

            // Seed Vendedor User
            var vendedorUser = new Usuario(
                "vendedor", 
                new Email("vendedor@sistema.com"), 
                passwordHasher.HashPassword("VendedorPassword123!"), 
                RolUsuario.Vendedor
            );
            vendedorUser.RegistrarCreacion("Semilla");
            await context.Usuarios.AddAsync(vendedorUser);

            // Seed Operador User (Almacenero)
            var operadorUser = new Usuario(
                "operador", 
                new Email("operador@sistema.com"), 
                passwordHasher.HashPassword("OperadorPassword123!"), 
                RolUsuario.Almacenero
            );
            operadorUser.RegistrarCreacion("Semilla");
            await context.Usuarios.AddAsync(operadorUser);

            await context.SaveChangesAsync();
        }
    }
}
