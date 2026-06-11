using FluentValidation;
using InventorySalesApi.Application.Features.Productos.Commands;

namespace InventorySalesApi.Application.Validators;

public class CrearProductoCommandValidator : AbstractValidator<CrearProductoCommand>
{
    public CrearProductoCommandValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("El SKU del producto es requerido.")
            .MaximumLength(50).WithMessage("El SKU no puede superar los 50 caracteres.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del producto no puede estar vacío.")
            .MaximumLength(150).WithMessage("El nombre del producto no puede superar los 150 caracteres.");

        RuleFor(x => x.Precio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio del producto no puede ser negativo.");

        RuleFor(x => x.StockInicial)
            .GreaterThanOrEqualTo(0).WithMessage("El stock inicial no puede ser negativo.");

        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("Debe asociar el producto a una categoría válida.");
    }
}
