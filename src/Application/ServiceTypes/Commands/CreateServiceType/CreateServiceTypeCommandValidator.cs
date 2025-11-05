namespace MigratingAssistant.Application.ServiceTypes.Commands.CreateServiceType;

public class CreateServiceTypeCommandValidator : AbstractValidator<CreateServiceTypeCommand>
{
    public CreateServiceTypeCommandValidator()
    {
        RuleFor(v => v.ServiceKey)
            .NotEmpty().WithMessage("ServiceKey is required.")
            .MaximumLength(200).WithMessage("ServiceKey must not exceed 200 characters.");

        RuleFor(v => v.DisplayName)
            .MaximumLength(500).WithMessage("DisplayName must not exceed 500 characters.");
    }
}
