using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.JobApplications.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.JobApplications.Commands;

[TestFixture]
public class UpdateJobApplicationCommandHandlerTests : SharedUnitTestBase
{
    private UpdateJobApplicationCommandHandler _handler = null!;
    private Mock<IRepository<JobApplication>> _jobApplicationRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _jobApplicationRepository = new Mock<IRepository<JobApplication>>();
        UnitOfWork.Setup(x => x.JobApplications).Returns(_jobApplicationRepository.Object);
        _handler = new UpdateJobApplicationCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_EmployerReviewsApplication_ShouldUpdateStatus()
    {
        // Arrange - Business context: Employer reviews application
        var applicationId = Guid.NewGuid();
        var existingApplication = new JobApplication
        {
            Id = applicationId,
            JobId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Status = JobApplicationSubmissionStatus.Submitted
        };

        var command = new UpdateJobApplicationCommand
        {
            Id = applicationId,
            Status = JobApplicationSubmissionStatus.Reviewed
        };

        _jobApplicationRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingApplication);

        JobApplication? updatedApplication = null;
        _jobApplicationRepository
            .Setup(x => x.Update(It.IsAny<JobApplication>()))
            .Callback<JobApplication>(ja => updatedApplication = ja);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedApplication.Should().NotBeNull();
        updatedApplication!.Status.Should().Be(JobApplicationSubmissionStatus.Reviewed);
        _jobApplicationRepository.Verify(x => x.Update(It.IsAny<JobApplication>()), Times.Once);
    }

    [Test]
    public void Handle_ApplicationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var command = new UpdateJobApplicationCommand
        {
            Id = applicationId,
            Status = JobApplicationSubmissionStatus.Accepted
        };

        _jobApplicationRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobApplication?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));
    }
}
