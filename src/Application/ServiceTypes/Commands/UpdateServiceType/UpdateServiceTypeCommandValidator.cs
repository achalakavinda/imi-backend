namespace MigratingAssistant.Application.ServiceTypes.Commands.UpdateServiceType;

public class UpdateServiceTypeCommandValidator : AbstractValidator<UpdateServiceTypeCommand>
{
    public UpdateServiceTypeCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(v => v.ServiceKey)
            .NotEmpty().WithMessage("ServiceKey is required.")
            .MaximumLength(200).WithMessage("ServiceKey must not exceed 200 characters.");

        RuleFor(v => v.DisplayName)
            .MaximumLength(500).WithMessage("DisplayName must not exceed 500 characters.");
    }
}
