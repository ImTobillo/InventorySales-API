namespace InventorySalesApi.Domain.Enums;

/// <summary>
/// Representa los distintos tipos de movimientos de stock.
/// </summary>
public enum TipoMovimiento
{
    /// <summary>
    /// Entrada de productos al almacén (compras, devoluciones de clientes, etc.).
    /// </summary>
    Ingreso = 1,

    /// <summary>
    /// Salida de productos del almacén (ventas, mermas, devoluciones a proveedores, etc.).
    /// </summary>
    Egreso = 2,

    /// <summary>
    /// Ajuste manual de inventario para corregir discrepancias.
    /// </summary>
    Ajuste = 3
}
