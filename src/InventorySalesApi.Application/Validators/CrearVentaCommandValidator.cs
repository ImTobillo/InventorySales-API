using FluentValidation;
using InventorySalesApi.Application.Features.Ventas.Commands;

namespace InventorySalesApi.Application.Validators;

public class CrearVentaCommandValidator : AbstractValidator<CrearVentaCommand>
{
    public CrearVentaCommandValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("El ID del cliente es requerido.");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El ID del usuario vendedor es requerido.");

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("La venta debe contener al menos un detalle de producto.")
            .Must(d => d != null && d.Count > 0).WithMessage("La venta debe contener al menos un detalle de producto.");

        RuleForEach(x => x.Detalles).SetValidator(new CrearDetalleVentaDtoValidator());
    }
}

public class CrearDetalleVentaDtoValidator : AbstractValidator<CrearDetalleVentaDto>
{
    public CrearDetalleVentaDtoValidator()
    {
        RuleFor(x => x.ProductoId)
            .NotEmpty().WithMessage("El ID del producto es requerido.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad vendida debe ser mayor a cero.");

        RuleFor(x => x.Descuento)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo.");
    }
}
