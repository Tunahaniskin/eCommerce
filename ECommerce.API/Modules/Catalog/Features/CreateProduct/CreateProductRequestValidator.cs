using FluentValidation;

namespace ECommerce.API.Modules.Catalog.Features.CreateProduct;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .Length(2, 200).WithMessage("Ürün adı 2 ile 200 karakter arasında olmalıdır.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.")
            .LessThan(10_000_000).WithMessage("Fiyat 10.000.000 sınırını aşamaz.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz.")
            .LessThan(1_000_000).WithMessage("Stok adedi 1.000.000 sınırını aşamaz.");
    }
}