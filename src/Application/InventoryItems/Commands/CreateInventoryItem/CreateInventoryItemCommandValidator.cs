namespace MigratingAssistant.Application.InventoryItems.Commands.CreateInventoryItem;

public class CreateInventoryItemCommandValidator : AbstractValidator<CreateInventoryItemCommand>
{
    public CreateInventoryItemCommandValidator()
    {
        RuleFor(v => v.ListingId)
            .NotEmpty().WithMessage("ListingId is required.");

        RuleFor(v => v.Sku)
            .MaximumLength(200).WithMessage("Sku must not exceed 200 characters.");
    }
}
