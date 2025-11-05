using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Application.Users.Commands;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.Users.Commands;

[TestFixture]
public class UpdateUserCommandHandlerTests : SharedUnitTestBase
{
    private UpdateUserCommandHandler _handler = null!;
    private Mock<IRepository<User>> _userRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();

        _userRepository = new Mock<IRepository<User>>();
        UnitOfWork.Setup(x => x.Users).Returns(_userRepository.Object);

        _handler = new UpdateUserCommandHandler(UnitOfWork.Object);
    }

    #region Handle Method Tests

    [Test]
    public async Task Handle_WithValidCommand_ShouldUpdateUserAndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Email = "old@example.com",
            Role = UserRole.User,
            PasswordHash = "hash123"
        };

        var command = new UpdateUserCommand
        {
            Id = userId,
            Email = "new@example.com",
            Role = UserRole.Provider
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        existingUser.Email.Should().Be("new@example.com");
        existingUser.Role.Should().Be(UserRole.Provider);

        _userRepository.Verify(x => x.GetByIdAsync(userId, CancellationToken), Times.Once);
        _userRepository.Verify(x => x.Update(existingUser), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_WithOnlyEmailUpdate_ShouldUpdateEmailOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Email = "old@example.com",
            Role = UserRole.User
        };

        var command = new UpdateUserCommand
        {
            Id = userId,
            Email = "updated@example.com",
            Role = null // Not updating role
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        existingUser.Email.Should().Be("updated@example.com");
        existingUser.Role.Should().Be(UserRole.User); // Should remain unchanged

        _userRepository.Verify(x => x.Update(existingUser), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public void Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            Id = userId,
            Email = "new@example.com"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("User");
        ex.Message.Should().Contain(userId.ToString());

        _userRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_WhenGetByIdFails_ShouldNotCallUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand
        {
            Id = userId,
            Email = "new@example.com"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        _userRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_WhenSaveChangesFails_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Email = "old@example.com"
        };

        var command = new UpdateUserCommand
        {
            Id = userId,
            Email = "new@example.com"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Be("Save failed");
        _userRepository.Verify(x => x.Update(existingUser), Times.Once);
    }

    [Test]
    public async Task Handle_WithRoleUpgradeToProvider_ShouldUpdateSuccessfully()
    {
        // Arrange - Business scenario: Regular user becoming a service provider
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Email = "user@example.com",
            Role = UserRole.User,
            EmailVerified = true
        };

        var command = new UpdateUserCommand
        {
            Id = userId,
            Email = null, // Not updating email
            Role = UserRole.Provider // Upgrading to provider role
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        existingUser.Role.Should().Be(UserRole.Provider);
        // Email should not be set to null when command.Email is null

        _userRepository.Verify(x => x.Update(existingUser), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    #endregion
}
