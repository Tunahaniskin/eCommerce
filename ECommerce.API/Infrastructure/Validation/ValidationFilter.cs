using FluentValidation;

namespace ECommerce.API.Infrastructure.Validation;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T>? _validator;

    public ValidationFilter(IValidator<T>? validator = null)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (_validator is null)
            return await next(context);

        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null)
            return Results.BadRequest(new { Message = "İstek gövdesi boş olamaz." });

        var validationResult = await _validator.ValidateAsync(argument, context.HttpContext.RequestAborted);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}