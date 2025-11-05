using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Application.UserProfiles.Commands;
using MigratingAssistant.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.UserProfiles.Commands;

[TestFixture]
public class DeleteUserProfileCommandHandlerTests : SharedUnitTestBase
{
    private Mock<IRepository<UserProfile>> _mockUserProfileRepository = null!;
    private DeleteUserProfileCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _mockUserProfileRepository = new Mock<IRepository<UserProfile>>();
        UnitOfWork.Setup(u => u.UserProfiles).Returns(_mockUserProfileRepository.Object);
        _handler = new DeleteUserProfileCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidUserProfileId_DeletesUserProfileSuccessfully()
    {
        // Arrange - Business scenario: Delete an incomplete user profile
        var userProfileId = Guid.NewGuid();

        var userProfile = new UserProfile
        {
            Id = userProfileId,
            FirstName = "John",
            LastName = "Doe",
            Phone = "+1234567890",
            Nationality = "USA"
        };

        _mockUserProfileRepository
            .Setup(r => r.GetByIdAsync(userProfileId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var command = new DeleteUserProfileCommand(userProfileId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockUserProfileRepository.Verify(r => r.Delete(It.Is<UserProfile>(up => up.Id == userProfileId)), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_NonExistentUserProfileId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _mockUserProfileRepository
            .Setup(r => r.GetByIdAsync(nonExistentId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile)null!);

        var command = new DeleteUserProfileCommand(nonExistentId);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken));

        ex.Message.Should().Contain(nameof(UserProfile));
        ex.Message.Should().Contain(nonExistentId.ToString());

        _mockUserProfileRepository.Verify(r => r.Delete(It.IsAny<UserProfile>()), Times.Never);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ProfileWithAllFields_AllowsDeletion()
    {
        // Arrange - Business scenario: Delete complete user profile
        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile
        {
            Id = userProfileId,
            FirstName = "Jane",
            LastName = "Smith",
            Phone = "+9876543210",
            Nationality = "Canada",
            Bio = "Software engineer and immigrant"
        };

        _mockUserProfileRepository
            .Setup(r => r.GetByIdAsync(userProfileId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var command = new DeleteUserProfileCommand(userProfileId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockUserProfileRepository.Verify(r => r.Delete(It.Is<UserProfile>(up =>
            up.Id == userProfileId &&
            up.FirstName == "Jane" &&
            up.LastName == "Smith"
        )), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ProfileWithMinimalInfo_AllowsDeletion()
    {
        // Arrange - Business scenario: Delete incomplete profile
        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile
        {
            Id = userProfileId,
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserProfileRepository
            .Setup(r => r.GetByIdAsync(userProfileId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var command = new DeleteUserProfileCommand(userProfileId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockUserProfileRepository.Verify(r => r.Delete(It.Is<UserProfile>(up =>
            up.Id == userProfileId &&
            string.IsNullOrEmpty(up.Phone)
        )), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidCommand_UsesCancellationToken()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile
        {
            Id = userProfileId,
            FirstName = "Cancel",
            LastName = "Test",
            Phone = "+1111111111"
        };

        _mockUserProfileRepository
            .Setup(r => r.GetByIdAsync(userProfileId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var command = new DeleteUserProfileCommand(userProfileId);
        var cancellationToken = new CancellationToken();

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockUserProfileRepository.Verify(r => r.GetByIdAsync(userProfileId.ToString(), cancellationToken), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }
}
