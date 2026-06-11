using FluentValidation;
using InventorySalesApi.Application.Features.Stock.Commands;

namespace InventorySalesApi.Application.Validators;

public class RegistrarEntradaStockCommandValidator : AbstractValidator<RegistrarEntradaStockCommand>
{
    public RegistrarEntradaStockCommandValidator()
    {
        RuleFor(x => x.ProductoId)
            .NotEmpty().WithMessage("El ID del producto es requerido.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad a ingresar debe ser mayor a cero.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción o motivo del movimiento es requerido.")
            .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El ID del usuario que registra el movimiento es requerido.");
    }
}
