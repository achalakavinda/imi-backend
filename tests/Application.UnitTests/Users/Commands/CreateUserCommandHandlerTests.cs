using AutoMapper;
using FluentAssertions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Application.Users.Commands;
using MigratingAssistant.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.Users.Commands;

[TestFixture]
public class CreateUserCommandHandlerTests : SharedUnitTestBase
{
    private CreateUserCommandHandler _handler = null!;
    private Mock<IRepository<User>> _userRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks(); // Reset all base mocks including SaveChangesAsync

        _userRepository = new Mock<IRepository<User>>();
        UnitOfWork.Setup(x => x.Users).Returns(_userRepository.Object);

        _handler = new CreateUserCommandHandler(UnitOfWork.Object);
    }

    #region Handle Method Tests

    [Test]
    public async Task Handle_WithValidCommand_ShouldInsertUserAndReturnId()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com"
        };

        User? capturedUser = null;
        _userRepository
            .Setup(x => x.Insert(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be("test@example.com");
        capturedUser.Id.Should().Be(result); // The handler returns the entity's Id

        _userRepository.Verify(x => x.Insert(It.IsAny<User>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public void Handle_WhenInsertFails_ShouldNotCallSaveChanges()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com"
        };

        _userRepository
            .Setup(x => x.Insert(It.IsAny<User>()))
            .Throws(new InvalidOperationException("Database error"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_WhenSaveChangesFails_ShouldThrowException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com"
        };

        // Override the default successful SaveChangesAsync setup
        UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Be("Save failed");
        _userRepository.Verify(x => x.Insert(It.IsAny<User>()), Times.Once);
    }

    #endregion
}
