using FluentValidation;
using CMS.Application.Features.Sites.Commands;

namespace CMS.Application.Features.Sites.Validators;

public class CreateSiteCommandValidator : AbstractValidator<CreateSiteCommand>
{
    public CreateSiteCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Site name is required")
            .MaximumLength(200).WithMessage("Site name cannot exceed 200 characters")
            .MinimumLength(1).WithMessage("Site name must be at least 1 character");

        RuleFor(x => x.Domain)
            .NotEmpty().WithMessage("Domain is required")
            .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9-\.]{0,61}[a-zA-Z0-9]?\.[a-zA-Z]{2,}$")
            .WithMessage("Invalid domain format. Example: example.com")
            .MaximumLength(255).WithMessage("Domain cannot exceed 255 characters");
    }
}
