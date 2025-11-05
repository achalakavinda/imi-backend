namespace MigratingAssistant.Application.Jobs.Commands.CreateJob;

public class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(v => v.ListingId)
            .NotEmpty().WithMessage("ListingId is required.");

        RuleFor(v => v.JobType)
            .MaximumLength(200).WithMessage("JobType must not exceed 200 characters.");
    }
}
