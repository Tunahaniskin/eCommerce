using FluentValidation;

namespace ECommerce.API.Modules.Catalog.Features.GetProducts;

public class GetProductsRequestValidator : AbstractValidator<GetProductsRequest>
{
    public GetProductsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası 1 veya daha büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Sayfa boyutu en az 1, en fazla 50 olabilir.");
    }
}