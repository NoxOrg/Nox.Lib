using FluentValidation;
using Nox.Core.Interfaces.Entity;

namespace Nox.Core.Validation;

public class EntityValidator : AbstractValidator<IEntity>
{
    public EntityValidator()
    {
        RuleFor(entity => entity.Name)
            .NotEmpty()
            .WithMessage(entity => $"The entity's name must be specified in {entity.DefinitionFileName}");

        RuleFor(entity => entity.ApplyDefaults())
            .NotEqual(false)
            .WithMessage(entity => $"Defaults could not be applied to entity defined in {entity.DefinitionFileName}");

        RuleFor(entity => entity.Attributes)
            .NotEmpty()
            .WithMessage(entity => $"The entity must have at least one property defined in {entity.DefinitionFileName}");

        RuleForEach(entity => entity.Attributes)
            .SetValidator(new EntityAttributeValidator());

        RuleFor(service => service.Key)
            .SetValidator(new EntityKeyValidator());
    }
}