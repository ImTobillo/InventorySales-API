namespace InventorySalesApi.Domain.Enums;

/// <summary>
/// Representa el rol o nivel de acceso de un usuario en el sistema.
/// </summary>
public enum RolUsuario
{
    /// <summary>
    /// Acceso total al sistema y configuraciones.
    /// </summary>
    Administrador = 1,

    /// <summary>
    /// Rol enfocado en realizar ventas y gestionar clientes.
    /// </summary>
    Vendedor = 2,

    /// <summary>
    /// Rol enfocado en gestionar el inventario, productos, categorías y movimientos de stock.
    /// </summary>
    Almacenero = 3
}
