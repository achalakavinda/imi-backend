using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Application.UserProfiles.Commands;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.UserProfiles.Commands;

[TestFixture]
public class CreateUserProfileCommandHandlerTests : SharedUnitTestBase
{
    private CreateUserProfileCommandHandler _handler = null!;
    private Mock<IRepository<UserProfile>> _userProfileRepository = null!;
    private Mock<IRepository<User>> _userRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _userProfileRepository = new Mock<IRepository<UserProfile>>();
        _userRepository = new Mock<IRepository<User>>();
        UnitOfWork.Setup(x => x.UserProfiles).Returns(_userProfileRepository.Object);
        UnitOfWork.Setup(x => x.Users).Returns(_userRepository.Object);
        _handler = new CreateUserProfileCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_NewImmigrantProfile_ShouldCreateProfile()
    {
        // Arrange - Business context: New user completes profile after registration
        var userId = Guid.NewGuid();
        var command = new CreateUserProfileCommand
        {
            UserId = userId,
            FirstName = "Raj",
            LastName = "Kumar",
            Phone = "+61412345678",
            Nationality = "India",
            Bio = "International student at University of Sydney",
            Preferences = "{\"language\":\"en\",\"currency\":\"AUD\",\"notifications\":true}"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "raj.kumar@example.com", Role = UserRole.User });

        UserProfile? capturedProfile = null;
        _userProfileRepository
            .Setup(x => x.Insert(It.IsAny<UserProfile>()))
            .Callback<UserProfile>(p => capturedProfile = p);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedProfile.Should().NotBeNull();
        capturedProfile!.FirstName.Should().Be("Raj");
        capturedProfile.LastName.Should().Be("Kumar");
        capturedProfile.Nationality.Should().Be("India");
        capturedProfile.Preferences.Should().Contain("language", "User preferences stored for personalization");
        capturedProfile.Id.Should().Be(result);
    }

    [Test]
    public void Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateUserProfileCommand { UserId = userId, FirstName = "Test" };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));
        _userProfileRepository.Verify(x => x.Insert(It.IsAny<UserProfile>()), Times.Never);
    }
}
