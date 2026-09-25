using FluentValidation;

namespace ECommerce.API.Modules.Product.Features.GetProducts;

public class GetProductsRequestValidator : AbstractValidator<GetProductsRequest>
{
    public GetProductsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası 1 veya daha büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Sayfa boyutu en az 1, en fazla 50 olabilir.");

        // Status alanının geçerliliğini denetleme
        RuleFor(x => x.Status)
            .Must(s => string.IsNullOrEmpty(s) || s.ToLower() == "active" || s.ToLower() == "deleted" || s.ToLower() == "all")
            .WithMessage("Status parametresi sadece 'active', 'deleted' veya 'all' değerlerini alabilir.");
    }
}
