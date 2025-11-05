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
public class CreateJobApplicationCommandHandlerTests : SharedUnitTestBase
{
    private CreateJobApplicationCommandHandler _handler = null!;
    private Mock<IRepository<JobApplication>> _jobApplicationRepository = null!;
    private Mock<IRepository<User>> _userRepository = null!;
    private Mock<IRepository<Job>> _jobRepository = null!;
    private Mock<IRepository<Document>> _documentRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _jobApplicationRepository = new Mock<IRepository<JobApplication>>();
        _userRepository = new Mock<IRepository<User>>();
        _jobRepository = new Mock<IRepository<Job>>();
        _documentRepository = new Mock<IRepository<Document>>();
        UnitOfWork.Setup(x => x.JobApplications).Returns(_jobApplicationRepository.Object);
        UnitOfWork.Setup(x => x.Users).Returns(_userRepository.Object);
        UnitOfWork.Setup(x => x.Jobs).Returns(_jobRepository.Object);
        UnitOfWork.Setup(x => x.Documents).Returns(_documentRepository.Object);
        _handler = new CreateJobApplicationCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ImmigrantAppliesForJob_ShouldCreateApplication()
    {
        // Arrange - Business context: Immigrant applies for job listing
        var userId = Guid.NewGuid();
        var jobId = Guid.NewGuid();
        var resumeId = Guid.NewGuid();

        var command = new CreateJobApplicationCommand
        {
            UserId = userId,
            JobId = jobId,
            ResumeFileId = resumeId,
            Status = JobApplicationSubmissionStatus.Submitted
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "jobseeker@example.com", Role = UserRole.User });

        _jobRepository
            .Setup(x => x.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Job { Id = jobId, JobType = "Software Developer" });

        _documentRepository
            .Setup(x => x.GetByIdAsync(resumeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document { Id = resumeId, DocType = "Resume", UserId = userId });

        JobApplication? capturedApplication = null;
        _jobApplicationRepository
            .Setup(x => x.Insert(It.IsAny<JobApplication>()))
            .Callback<JobApplication>(ja => capturedApplication = ja);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedApplication.Should().NotBeNull();
        capturedApplication!.UserId.Should().Be(userId);
        capturedApplication.JobId.Should().Be(jobId);
        capturedApplication.ResumeFileId.Should().Be(resumeId);
        capturedApplication.Status.Should().Be(JobApplicationSubmissionStatus.Submitted);
        capturedApplication.Id.Should().Be(result);
        _jobApplicationRepository.Verify(x => x.Insert(It.IsAny<JobApplication>()), Times.Once);
    }

    [Test]
    public void Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateJobApplicationCommand
        {
            UserId = Guid.NewGuid(),
            JobId = Guid.NewGuid(),
            Status = JobApplicationSubmissionStatus.Submitted
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));
    }

    [Test]
    public void Handle_JobNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateJobApplicationCommand
        {
            UserId = userId,
            JobId = Guid.NewGuid(),
            Status = JobApplicationSubmissionStatus.Submitted
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        _jobRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));
    }
}
