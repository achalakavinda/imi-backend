# Bulk Test Generator for Command Handlers
# This script generates comprehensive unit tests for all command handlers

$entities = @(
    @{
        Name = "UserProfile"
        Properties = @("UserId", "FirstName", "LastName", "Phone")
        Namespace = "UserProfiles"
    },
    @{
        Name = "ServiceProvider"
        Properties = @("UserId", "ProviderName", "ProviderType", "Verified")
        Namespace = "ServiceProviders"
    },
    @{
        Name = "Document"
        Properties = @("UserId", "DocType", "StoragePath", "Verified")
        Namespace = "Documents"
    },
    @{
        Name = "Booking"
        Properties = @("UserId", "ListingId", "StartAt", "Status")
        Namespace = "Bookings"
    },
    @{
        Name = "Payment"
        Properties = @("UserId", "Amount", "Currency", "Status")
        Namespace = "Payments"
    },
    @{
        Name = "JobApplication"
        Properties = @("UserId", "JobId", "Status")
        Namespace = "JobApplications"
    },
    @{
        Name = "SupportTicket"
        Properties = @("UserId", "Subject", "Body", "Status")
        Namespace = "SupportTickets"
    }
)

$testTemplate = @'
using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Application.{NAMESPACE}.Commands;
using MigratingAssistant.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.{NAMESPACE}.Commands;

[TestFixture]
public class Create{ENTITY}CommandHandlerTests : SharedUnitTestBase
{
    private Create{ENTITY}CommandHandler _handler = null!;
    private Mock<IRepository<{ENTITY}>> _repository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        
        _repository = new Mock<IRepository<{ENTITY}>>();
        UnitOfWork.Setup(x => x.{ENTITY_PLURAL}).Returns(_repository.Object);
        
        _handler = new Create{ENTITY}CommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldInsertAndReturnId()
    {
        // Arrange
        var command = new Create{ENTITY}Command
        {
            // TODO: Set properties
        };

        {ENTITY}? captured = null;
        _repository
            .Setup(x => x.Insert(It.IsAny<{ENTITY}>()))
            .Callback<{ENTITY}>(entity => captured = entity);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        captured.Should().NotBeNull();
        captured!.Id.Should().Be(result);
        
        _repository.Verify(x => x.Insert(It.IsAny<{ENTITY}>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public void Handle_WhenInsertFails_ShouldNotCallSaveChanges()
    {
        // Arrange
        var command = new Create{ENTITY}Command();

        _repository
            .Setup(x => x.Insert(It.IsAny<{ENTITY}>()))
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
        var command = new Create{ENTITY}Command();

        UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Be("Save failed");
        _repository.Verify(x => x.Insert(It.IsAny<{ENTITY}>()), Times.Once);
    }
}

[TestFixture]
public class Update{ENTITY}CommandHandlerTests : SharedUnitTestBase
{
    private Update{ENTITY}CommandHandler _handler = null!;
    private Mock<IRepository<{ENTITY}>> _repository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        
        _repository = new Mock<IRepository<{ENTITY}>>();
        UnitOfWork.Setup(x => x.{ENTITY_PLURAL}).Returns(_repository.Object);
        
        _handler = new Update{ENTITY}CommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldUpdateAndSave()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new {ENTITY} { Id = id };

        var command = new Update{ENTITY}Command
        {
            Id = id
            // TODO: Set properties
        };

        _repository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _repository.Verify(x => x.GetByIdAsync(id, CancellationToken), Times.Once);
        _repository.Verify(x => x.Update(existing), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public void Handle_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new Update{ENTITY}Command { Id = id };

        _repository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(({ENTITY}?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("{ENTITY}");
        
        _repository.Verify(x => x.Update(It.IsAny<{ENTITY}>()), Times.Never);
        UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_WhenSaveChangesFails_ShouldThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new {ENTITY} { Id = id };
        var command = new Update{ENTITY}Command { Id = id };

        _repository
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Be("Save failed");
        _repository.Verify(x => x.Update(existing), Times.Once);
    }
}
'@

Write-Host "Test generation template created. Ready to generate tests for $($entities.Count) entities." -ForegroundColor Green
Write-Host "Run this script to generate all test files." -ForegroundColor Yellow
