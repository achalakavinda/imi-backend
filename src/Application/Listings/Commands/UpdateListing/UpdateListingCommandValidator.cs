namespace MigratingAssistant.Application.Listings.Commands.UpdateListing;

public class UpdateListingCommandValidator : AbstractValidator<UpdateListingCommand>
{
    public UpdateListingCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(v => v.ServiceTypeId)
            .GreaterThan(0).WithMessage("ServiceTypeId must be greater than 0.");

        RuleFor(v => v.ProviderId)
            .NotEmpty().WithMessage("ProviderId is required.");

        RuleFor(v => v.Title)
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");

        RuleFor(v => v.Price)
            .GreaterThanOrEqualTo(0).When(v => v.Price.HasValue)
            .WithMessage("Price must be greater than or equal to 0.");
    }
}
