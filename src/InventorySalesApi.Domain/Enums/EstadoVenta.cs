namespace InventorySalesApi.Domain.Enums;

/// <summary>
/// Representa el estado actual de una venta.
/// </summary>
public enum EstadoVenta
{
    /// <summary>
    /// Venta creada y registrada, pendiente de confirmación de pago o entrega.
    /// </summary>
    Pendiente = 1,

    /// <summary>
    /// Venta finalizada correctamente.
    /// </summary>
    Completada = 2,

    /// <summary>
    /// Venta anulada o cancelada. El stock puede haber sido reintegrado.
    /// </summary>
    Cancelada = 3
}
