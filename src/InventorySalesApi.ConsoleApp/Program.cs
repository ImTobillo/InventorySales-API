using System;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.ValueObjects;
using InventorySalesApi.Domain.Enums;
using InventorySalesApi.Domain.Exceptions;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("==========================================================================");
Console.WriteLine("🚀 SIMULADOR DE DOMINIO - SISTEMA DE INVENTARIO Y VENTAS (.NET 10 Onion) 🚀");
Console.WriteLine("==========================================================================\n");

// 1. Inicialización de datos básicos de Auditoría
var usuarioAlmacenId = Guid.NewGuid();
var usuarioVendedorId = Guid.NewGuid();

Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine("1. Instanciando Datos Iniciales");
Console.WriteLine("--------------------------------------------------------------------------");

// Crear Categoría
var categoriaElectro = new Categoria("Electrodomésticos", "Dispositivos electrónicos del hogar");
categoriaElectro.RegistrarCreacion("admin_sistema");
Console.WriteLine($"✅ Categoría Creada: '{categoriaElectro.Nombre}' (ID: {categoriaElectro.Id})");

// Crear Producto con stock inicial
var productoId = Guid.NewGuid();
var precioNotebook = new Money(1200.50m);
var notebook = new Producto("NOTE-ASUS-UX340", "Notebook ASUS ZenBook 14", "Intel Core i7, 16GB RAM, 512GB SSD", precioNotebook, 10, categoriaElectro.Id);
notebook.RegistrarCreacion("admin_almacen");
Console.WriteLine($"✅ Producto Creado: '{notebook.Nombre}' | SKU: {notebook.Sku} | Stock Inicial: {notebook.Stock} | Precio: {notebook.Precio}");

// Crear Cliente
var emailCliente = new Email("juan.perez@example.com");
var clienteJuan = new Cliente("Juan", "Pérez", emailCliente, "+54 9 11 5555-4444", "DNI321456");
clienteJuan.RegistrarCreacion("vendedor_juan");
Console.WriteLine($"✅ Cliente Creado: '{clienteJuan.Nombre} {clienteJuan.Apellido}' | Documento: {clienteJuan.DocumentoIdentidad} | Email: {clienteJuan.Email}");


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("2. Validaciones de Reglas de Negocio en Objetos de Valor y Entidades");
Console.WriteLine("--------------------------------------------------------------------------");

// Intento de crear producto con precio negativo
try
{
    Console.Write("Intentando crear un producto con precio negativo (-150)... ");
    var precioInvalido = new Money(-150.00m);
}
catch (DomainException ex)
{
    Console.WriteLine($"❌ EXCEPCIÓN ESPERADA: {ex.Message}");
}

// Intento de crear un Email con formato inválido
try
{
    Console.Write("Intentando registrar un cliente con correo electrónico inválido ('juan.perez')... ");
    var emailInvalido = new Email("juan.perez");
}
catch (DomainException ex)
{
    Console.WriteLine($"❌ EXCEPCIÓN ESPERADA: {ex.Message}");
}


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("3. Gestión de Stock a través de Métodos de Dominio");
Console.WriteLine("--------------------------------------------------------------------------");

Console.WriteLine($"Stock actual de {notebook.Nombre}: {notebook.Stock}");
Console.WriteLine("Registrando ingreso de 5 unidades por reposición de proveedor...");
notebook.RegistrarMovimientoStock(5, TipoMovimiento.Ingreso, "Reposición de stock mensual - Proveedor Asus SRL", usuarioAlmacenId);
Console.WriteLine($"✅ Nuevo Stock: {notebook.Stock}");
Console.WriteLine($"Movimientos registrados: {notebook.MovimientosStock.Count}");
foreach (var mov in notebook.MovimientosStock)
{
    Console.WriteLine($"  - [{mov.Fecha:HH:mm:ss}] Tipo: {mov.Tipo} | Cantidad: {mov.Cantidad} | Razón: {mov.Descripcion}");
}


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("4. Simulación de una Venta (Flujo Completo)");
Console.WriteLine("--------------------------------------------------------------------------");

// Crear Venta
var venta = new Venta(clienteJuan.Id);
venta.RegistrarCreacion("vendedor_juan");
Console.WriteLine($"✅ Venta registrada (ID: {venta.Id}) en estado: {venta.Estado}");

// Intentar vender más stock del disponible (disponible: 15)
try
{
    Console.Write("Intentando vender 20 unidades del Notebook ASUS (disponible: 15)... ");
    venta.AgregarDetalle(notebook, 20, 0, usuarioVendedorId);
}
catch (DomainException ex)
{
    Console.WriteLine($"❌ EXCEPCIÓN ESPERADA: {ex.Message}");
}

// Venta válida: Agregar 2 Notebooks con un descuento de $100.00
Console.WriteLine("\nAgregando 2 Notebooks con un descuento total de $100.00 al detalle...");
venta.AgregarDetalle(notebook, 2, 100.00m, usuarioVendedorId);

Console.WriteLine($"✅ Venta Actualizada:");
Console.WriteLine($"  - Total Venta: {venta.Total}");
Console.WriteLine($"  - Stock Restante de {notebook.Nombre}: {notebook.Stock}");

// Mostrar detalles de la venta
foreach (var det in venta.Detalles)
{
    Console.WriteLine($"  Detalle Línea:");
    Console.WriteLine($"    - Producto: {det.Producto.Nombre}");
    Console.WriteLine($"    - Cantidad: {det.Cantidad}");
    Console.WriteLine($"    - Precio Unitario: {det.PrecioUnitario}");
    Console.WriteLine($"    - Descuento aplicado: {det.Descuento}");
    Console.WriteLine($"    - Subtotal Línea: {det.Subtotal}");
}


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("5. Completar la Venta");
Console.WriteLine("--------------------------------------------------------------------------");

venta.CompletarVenta();
Console.WriteLine($"✅ Estado de la venta después de completar: {venta.Estado}");


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("6. Cancelar la Venta y Reintegrar Stock (Lógica de Agregado)");
Console.WriteLine("--------------------------------------------------------------------------");

Console.WriteLine($"Stock antes de la cancelación: {notebook.Stock}");
Console.WriteLine("Cancelando la venta por solicitud de cliente...");
venta.CancelarVenta(usuarioVendedorId);
Console.WriteLine($"✅ Estado de la venta después de cancelar: {venta.Estado}");
Console.WriteLine($"✅ Stock actual de {notebook.Nombre} (debe ser 15): {notebook.Stock}");

// Mostrar todos los movimientos de stock del producto al final
Console.WriteLine("\nHistorial completo de movimientos de stock para el producto:");
foreach (var mov in notebook.MovimientosStock)
{
    Console.WriteLine($"  - [{mov.Fecha:HH:mm:ss}] Tipo: {mov.Tipo} | Cantidad: {mov.Cantidad} | Detalle: {mov.Descripcion}");
}

Console.WriteLine("\n==========================================================================");
Console.WriteLine("✨ SIMULACIÓN DE DOMINIO TERMINADA CON ÉXITO - TODO COMPORTAMIENTO OK ✨");
Console.WriteLine("==========================================================================");
