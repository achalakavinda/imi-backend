namespace MigratingAssistant.Application.Jobs.Commands.UpdateJob;

public class UpdateJobCommandValidator : AbstractValidator<UpdateJobCommand>
{
    public UpdateJobCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(v => v.ListingId)
            .NotEmpty().WithMessage("ListingId is required.");

        RuleFor(v => v.JobType)
            .MaximumLength(200).WithMessage("JobType must not exceed 200 characters.");
    }
}
