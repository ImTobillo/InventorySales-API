using FluentValidation;
using InventorySalesApi.Application.Features.Productos.Commands;

namespace InventorySalesApi.Application.Validators;

public class ActualizarProductoCommandValidator : AbstractValidator<ActualizarProductoCommand>
{
    public ActualizarProductoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El ID del producto es requerido.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del producto no puede estar vacío.")
            .MaximumLength(150).WithMessage("El nombre del producto no puede superar los 150 caracteres.");

        RuleFor(x => x.Precio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio del producto no puede ser negativo.");

        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("Debe asociar el producto a una categoría válida.");
    }
}
