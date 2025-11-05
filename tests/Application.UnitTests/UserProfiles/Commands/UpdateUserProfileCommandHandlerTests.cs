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
public class UpdateUserProfileCommandHandlerTests : SharedUnitTestBase
{
    private UpdateUserProfileCommandHandler _handler = null!;
    private Mock<IRepository<UserProfile>> _userProfileRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _userProfileRepository = new Mock<IRepository<UserProfile>>();
        UnitOfWork.Setup(x => x.UserProfiles).Returns(_userProfileRepository.Object);
        _handler = new UpdateUserProfileCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_UserUpdatesProfile_ShouldUpdateDetails()
    {
        // Arrange - User updates contact information
        var profileId = Guid.NewGuid();
        var existingProfile = new UserProfile
        {
            Id = profileId,
            UserId = Guid.NewGuid(),
            FirstName = "Raj",
            LastName = "Kumar",
            Phone = "+61412345678"
        };

        var command = new UpdateUserProfileCommand
        {
            Id = profileId,
            Phone = "+61499999999",
            Bio = "Updated bio - Moving to Melbourne"
        };

        _userProfileRepository
            .Setup(x => x.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProfile);

        UserProfile? updatedProfile = null;
        _userProfileRepository
            .Setup(x => x.Update(It.IsAny<UserProfile>()))
            .Callback<UserProfile>(p => updatedProfile = p);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedProfile.Should().NotBeNull();
        updatedProfile!.Phone.Should().Be("+61499999999");
        updatedProfile.Bio.Should().Be("Updated bio - Moving to Melbourne");
        _userProfileRepository.Verify(x => x.Update(It.IsAny<UserProfile>()), Times.Once);
    }

    [Test]
    public void Handle_ProfileNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand { Id = profileId, Phone = "+61400000000" };

        _userProfileRepository
            .Setup(x => x.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));
    }
}
