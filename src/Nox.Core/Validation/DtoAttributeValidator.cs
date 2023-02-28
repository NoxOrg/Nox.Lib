using FluentValidation;
using Nox.Core.Interfaces.Dto;

namespace Nox.Core.Validation;

public class DtoAttributeValidator: AbstractValidator<IDtoAttribute>
{
    public DtoAttributeValidator()
    {
        RuleFor(attribute => attribute.Name)
            .NotEmpty()
            .WithMessage(attribute => $"The attribute name must be specified in {attribute.DefinitionFileName}");

        RuleFor(attribute => attribute.Type)
            .NotEmpty()
            .WithMessage(attribute => $"Attribute '{attribute.Name}' has no type defined in {attribute.DefinitionFileName}");

        RuleFor(attribute => attribute.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(attribute => $"Defaults could not be applied to attributes defined in {attribute.DefinitionFileName}");
    }
}