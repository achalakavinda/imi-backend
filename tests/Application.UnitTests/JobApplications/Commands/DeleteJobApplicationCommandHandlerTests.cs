using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.JobApplications.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.JobApplications.Commands;

[TestFixture]
public class DeleteJobApplicationCommandHandlerTests : SharedUnitTestBase
{
    private Mock<IRepository<JobApplication>> _mockJobApplicationRepository = null!;
    private DeleteJobApplicationCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _mockJobApplicationRepository = new Mock<IRepository<JobApplication>>();
        UnitOfWork.Setup(u => u.JobApplications).Returns(_mockJobApplicationRepository.Object);
        _handler = new DeleteJobApplicationCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidJobApplicationId_DeletesJobApplicationSuccessfully()
    {
        // Arrange - Business scenario: Delete a rejected job application
        var jobApplicationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var jobId = Guid.NewGuid();

        var jobApplication = new JobApplication
        {
            Id = jobApplicationId,
            UserId = userId,
            JobId = jobId,
            Status = Domain.Enums.JobApplicationSubmissionStatus.Rejected,
            AppliedAt = DateTimeOffset.UtcNow.AddDays(-30)
        };

        _mockJobApplicationRepository
            .Setup(r => r.GetByIdAsync(jobApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobApplication);

        var command = new DeleteJobApplicationCommand(jobApplicationId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockJobApplicationRepository.Verify(r => r.Delete(It.Is<JobApplication>(ja => ja.Id == jobApplicationId)), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_NonExistentJobApplicationId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _mockJobApplicationRepository
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobApplication)null!);

        var command = new DeleteJobApplicationCommand(nonExistentId);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken));

        ex.Message.Should().Contain(nameof(JobApplication));
        ex.Message.Should().Contain(nonExistentId.ToString());

        _mockJobApplicationRepository.Verify(r => r.Delete(It.IsAny<JobApplication>()), Times.Never);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_PendingApplication_AllowsDeletion()
    {
        // Arrange - Business scenario: User withdraws pending application
        var jobApplicationId = Guid.NewGuid();
        var jobApplication = new JobApplication
        {
            Id = jobApplicationId,
            UserId = Guid.NewGuid(),
            JobId = Guid.NewGuid(),
            Status = Domain.Enums.JobApplicationSubmissionStatus.Submitted,
            AppliedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _mockJobApplicationRepository
            .Setup(r => r.GetByIdAsync(jobApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobApplication);

        var command = new DeleteJobApplicationCommand(jobApplicationId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockJobApplicationRepository.Verify(r => r.Delete(It.Is<JobApplication>(ja =>
            ja.Id == jobApplicationId &&
            ja.Status == Domain.Enums.JobApplicationSubmissionStatus.Submitted
        )), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_AcceptedApplication_AllowsDeletion()
    {
        // Arrange - Business scenario: Clean up old accepted applications
        var jobApplicationId = Guid.NewGuid();
        var jobApplication = new JobApplication
        {
            Id = jobApplicationId,
            UserId = Guid.NewGuid(),
            JobId = Guid.NewGuid(),
            Status = Domain.Enums.JobApplicationSubmissionStatus.Accepted,
            AppliedAt = DateTimeOffset.UtcNow.AddMonths(-6)
        };

        _mockJobApplicationRepository
            .Setup(r => r.GetByIdAsync(jobApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobApplication);

        var command = new DeleteJobApplicationCommand(jobApplicationId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockJobApplicationRepository.Verify(r => r.Delete(It.Is<JobApplication>(ja =>
            ja.Id == jobApplicationId &&
            ja.Status == Domain.Enums.JobApplicationSubmissionStatus.Accepted
        )), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidCommand_UsesCancellationToken()
    {
        // Arrange
        var jobApplicationId = Guid.NewGuid();
        var jobApplication = new JobApplication
        {
            Id = jobApplicationId,
            UserId = Guid.NewGuid(),
            JobId = Guid.NewGuid(),
            Status = Domain.Enums.JobApplicationSubmissionStatus.Submitted,
            AppliedAt = DateTimeOffset.UtcNow
        };

        _mockJobApplicationRepository
            .Setup(r => r.GetByIdAsync(jobApplicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobApplication);

        var command = new DeleteJobApplicationCommand(jobApplicationId);
        var cancellationToken = new CancellationToken();

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockJobApplicationRepository.Verify(r => r.GetByIdAsync(jobApplicationId, cancellationToken), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }
}
