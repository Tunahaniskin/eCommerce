using FluentValidation;

namespace ECommerce.API.Modules.Order.Features.CreateOrder;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Sipariş kalemleri boş olamaz.")
            .NotEmpty().WithMessage("Sepette en az bir ürün bulunmalıdır.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Geçersiz ürün kimliği.")
                .NotEqual(Guid.Empty).WithMessage("ProductId boş (empty) Guid olamaz.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Ürün adedi 0'dan büyük olmalıdır.")
                .LessThanOrEqualTo(50).WithMessage("Tek kalemde en fazla 50 adet sipariş verilebilir.");
        });
    }
}