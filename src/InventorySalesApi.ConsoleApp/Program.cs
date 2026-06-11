using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MediatR;
using FluentValidation;
using InventorySalesApi.Application.DependencyInjection;
using InventorySalesApi.Infrastructure;
using Infrastructure.Persistence;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.ValueObjects;
using InventorySalesApi.Domain.Enums;
using InventorySalesApi.Domain.Exceptions;
using InventorySalesApi.Application.Features.Productos.Commands;
using InventorySalesApi.Application.Features.Productos.Queries;
using InventorySalesApi.Application.Features.Clientes.Commands;
using InventorySalesApi.Application.Features.Clientes.Queries;
using InventorySalesApi.Application.Features.Ventas.Commands;
using InventorySalesApi.Application.Features.Ventas.Queries;
using InventorySalesApi.Application.Features.Stock.Commands;
using InventorySalesApi.Application.Features.Stock.Queries;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("==========================================================================");
Console.WriteLine("🚀 SIMULADOR DE CLEAN ARCHITECTURE - SISTEMA DE INVENTARIO Y VENTAS (.NET 10) 🚀");
Console.WriteLine("==========================================================================\n");

// 1. Configurar Contenedor de Dependencias (IoC)
var services = new ServiceCollection();

// Configurar base de datos en memoria para EF Core
services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("InventorySalesDb"));

// Registrar capas de la Arquitectura
services.AddApplicationServices();
services.AddInfrastructureServices();

var serviceProvider = services.BuildServiceProvider();

// 2. Inicialización de Datos Iniciales (Seed / Carga de auditoría básica)
using (var scope = serviceProvider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Crear Categorías iniciales
    var catElectro = new Categoria("Electrodomésticos", "Dispositivos electrónicos del hogar");
    catElectro.RegistrarCreacion("Semilla");
    await context.Categorias.AddAsync(catElectro);

    // Crear Usuarios (Almacén y Vendedor)
    var usuarioAlmacen = new Usuario("admin_almacen", new Email("almacen@sistema.com"), "hash_password_1", RolUsuario.Administrador);
    usuarioAlmacen.RegistrarCreacion("Semilla");
    await context.Usuarios.AddAsync(usuarioAlmacen);

    var usuarioVendedor = new Usuario("vendedor_juan", new Email("juan.vendedor@sistema.com"), "hash_password_2", RolUsuario.Vendedor);
    usuarioVendedor.RegistrarCreacion("Semilla");
    await context.Usuarios.AddAsync(usuarioVendedor);

    await context.SaveChangesAsync();
}

// 3. Ejecución del Flujo de Casos de Uso a través de MediatR
var mediator = serviceProvider.GetRequiredService<IMediator>();

// Obtener los IDs sembrados
Guid categoriaId;
Guid usuarioAlmacenId;
Guid usuarioVendedorId;

using (var scope = serviceProvider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    categoriaId = context.Categorias.First().Id;
    usuarioAlmacenId = context.Usuarios.First(u => u.NombreUsuario == "admin_almacen").Id;
    usuarioVendedorId = context.Usuarios.First(u => u.NombreUsuario == "vendedor_juan").Id;
}

Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine("1. Crear Productos a través de Casos de Uso (Commands)");
Console.WriteLine("--------------------------------------------------------------------------");

var crearProductoCmd = new CrearProductoCommand(
    Sku: "NOTE-ASUS-UX340",
    Nombre: "Notebook ASUS ZenBook 14",
    Descripcion: "Intel Core i7, 16GB RAM, 512GB SSD",
    Precio: 1200.50m,
    StockInicial: 10,
    CategoriaId: categoriaId
);

var notebookId = await mediator.Send(crearProductoCmd);
Console.WriteLine($"✅ Producto creado exitosamente por Caso de Uso. ID: {notebookId}");

// Intentar crear producto inválido (Validación con FluentValidation / Excepciones de Dominio)
try
{
    Console.Write("\nIntentando crear un producto con precio negativo (-150)... ");
    var cmdInvalido = crearProductoCmd with { Precio = -150.00m };
    
    // Ejecutar validación manual para FluentValidation primero
    var validator = serviceProvider.GetRequiredService<IValidator<CrearProductoCommand>>();
    var validationResult = await validator.ValidateAsync(cmdInvalido);
    if (!validationResult.IsValid)
    {
        throw new ValidationException(validationResult.Errors);
    }

    await mediator.Send(cmdInvalido);
}
catch (ValidationException ex)
{
    Console.WriteLine($"❌ VALIDACIÓN FLUENTVALIDATION CAPTURADA: {ex.Errors.First().ErrorMessage}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ EXCEPCIÓN: {ex.Message}");
}


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("2. Crear Clientes a través de Casos de Uso (Commands)");
Console.WriteLine("--------------------------------------------------------------------------");

var crearClienteCmd = new CrearClienteCommand(
    Nombre: "Juan",
    Apellido: "Pérez",
    Email: "juan.perez@example.com",
    Telefono: "+54 9 11 5555-4444",
    DocumentoIdentidad: "DNI321456"
);

var clienteId = await mediator.Send(crearClienteCmd);
Console.WriteLine($"✅ Cliente creado exitosamente por Caso de Uso. ID: {clienteId}");

// Intentar crear cliente con correo electrónico inválido
try
{
    Console.Write("\nIntentando registrar un cliente con correo electrónico inválido ('juan.perez')... ");
    var cmdInvalido = crearClienteCmd with { Email = "juan.perez" };
    
    var validator = serviceProvider.GetRequiredService<IValidator<CrearClienteCommand>>();
    var validationResult = await validator.ValidateAsync(cmdInvalido);
    if (!validationResult.IsValid)
    {
        throw new ValidationException(validationResult.Errors);
    }
    
    await mediator.Send(cmdInvalido);
}
catch (ValidationException ex)
{
    Console.WriteLine($"❌ VALIDACIÓN FLUENTVALIDATION CAPTURADA: {ex.Errors.First().ErrorMessage}");
}


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("3. Gestión de Stock a través de Casos de Uso");
Console.WriteLine("--------------------------------------------------------------------------");

// Consultar stock inicial del producto
var prodQuery = new ObtenerProductoPorIdQuery(notebookId);
var productoDto = await mediator.Send(prodQuery);
Console.WriteLine($"Stock actual de '{productoDto!.Nombre}': {productoDto.Stock}");

Console.WriteLine("Registrando ingreso de stock (Entrada de 5 unidades)...");
var entradaCmd = new RegistrarEntradaStockCommand(
    ProductoId: notebookId,
    Cantidad: 5,
    Descripcion: "Reposición de stock mensual - Proveedor Asus SRL",
    UsuarioId: usuarioAlmacenId
);
await mediator.Send(entradaCmd);

productoDto = await mediator.Send(prodQuery);
Console.WriteLine($"✅ Nuevo Stock del producto: {productoDto!.Stock}");


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("4. Simulación de una Venta Completa");
Console.WriteLine("--------------------------------------------------------------------------");

// Crear Venta
var detallesVenta = new List<CrearDetalleVentaDto>
{
    new CrearDetalleVentaDto(notebookId, 2, 100.00m) // 2 unidades con $100.00 de descuento total
};

var crearVentaCmd = new CrearVentaCommand(
    ClienteId: clienteId,
    Detalles: detallesVenta,
    UsuarioId: usuarioVendedorId
);

Console.WriteLine("Registrando una venta de 2 Notebooks...");
var ventaId = await mediator.Send(crearVentaCmd);
Console.WriteLine($"✅ Venta confirmada mediante Caso de Uso. ID: {ventaId}");

// Consultar información detallada de la venta
var ventaQuery = new ObtenerVentaPorIdQuery(ventaId);
var ventaDto = await mediator.Send(ventaQuery);

if (ventaDto != null)
{
    Console.WriteLine($"Detalles de la Venta:");
    Console.WriteLine($"  - Cliente: {ventaDto.ClienteNombreCompleto}");
    Console.WriteLine($"  - Total Venta: ${ventaDto.Total:N2}");
    Console.WriteLine($"  - Estado de Venta: {ventaDto.Estado}");
    foreach (var det in ventaDto.Detalles)
    {
        Console.WriteLine($"    * Item: {det.ProductoNombre} | Cantidad: {det.Cantidad} | Precio: ${det.PrecioUnitario:N2} | Descuento: ${det.Descuento:N2} | Subtotal: ${det.Subtotal:N2}");
    }
}

productoDto = await mediator.Send(prodQuery);
Console.WriteLine($"Stock restante de '{productoDto!.Nombre}' (disponible 15 - 2 vendidos = 13): {productoDto.Stock}");


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("5. Cancelación de Venta y Reintegro de Stock");
Console.WriteLine("--------------------------------------------------------------------------");

Console.WriteLine("Ejecutando cancelación de venta por solicitud del cliente...");
var cancelarVentaCmd = new CancelarVentaCommand(ventaId, usuarioVendedorId);
await mediator.Send(cancelarVentaCmd);
Console.WriteLine("✅ Venta Cancelada.");

ventaDto = await mediator.Send(ventaQuery);
Console.WriteLine($"  - Nuevo Estado de Venta: {ventaDto!.Estado}");

productoDto = await mediator.Send(prodQuery);
Console.WriteLine($"✅ Stock actual reintegrado (debe volver a 15): {productoDto!.Stock}");


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("6. Reporte Final de Movimientos de Stock (Queries)");
Console.WriteLine("--------------------------------------------------------------------------");

var movimientosQuery = new ObtenerMovimientosQuery(notebookId);
var movimientos = await mediator.Send(movimientosQuery);

Console.WriteLine($"Historial de movimientos para '{productoDto!.Nombre}':");
foreach (var mov in movimientos)
{
    Console.WriteLine($"  - [{mov.Fecha:HH:mm:ss}] Tipo: {mov.Tipo} | Cantidad: {mov.Cantidad} | Razón: {mov.Descripcion} | Por: {mov.UsuarioNombre}");
}

Console.WriteLine("\n==========================================================================");
Console.WriteLine("✨ SIMULACIÓN DE CASOS DE USO TERMINADA CON ÉXITO - ARQUITECTURA IMPECABLE ✨");
Console.WriteLine("==========================================================================");
