namespace MigratingAssistant.Application.InventoryItems.Commands.UpdateInventoryItem;

public class UpdateInventoryItemCommandValidator : AbstractValidator<UpdateInventoryItemCommand>
{
    public UpdateInventoryItemCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(v => v.ListingId)
            .NotEmpty().WithMessage("ListingId is required.");

        RuleFor(v => v.Sku)
            .MaximumLength(200).WithMessage("Sku must not exceed 200 characters.");
    }
}
