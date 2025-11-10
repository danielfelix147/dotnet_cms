using FluentValidation;
using CMS.Application.Features.Products.Commands;

namespace CMS.Application.Features.Products.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.SiteId)
            .NotEmpty().WithMessage("Site ID is required");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required")
            .MaximumLength(100).WithMessage("Product ID cannot exceed 100 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters")
            .MinimumLength(1).WithMessage("Product name must be at least 1 character");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative")
            .LessThanOrEqualTo(999999999).WithMessage("Price is too large");
    }
}
