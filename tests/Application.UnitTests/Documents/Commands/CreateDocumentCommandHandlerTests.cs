using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.Documents.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.Documents.Commands;

[TestFixture]
public class CreateDocumentCommandHandlerTests : SharedUnitTestBase
{
    private CreateDocumentCommandHandler _handler = null!;
    private Mock<IRepository<Document>> _documentRepository = null!;
    private Mock<IRepository<User>> _userRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _documentRepository = new Mock<IRepository<Document>>();
        _userRepository = new Mock<IRepository<User>>();
        UnitOfWork.Setup(x => x.Documents).Returns(_documentRepository.Object);
        UnitOfWork.Setup(x => x.Users).Returns(_userRepository.Object);
        _handler = new CreateDocumentCommandHandler(UnitOfWork.Object);
    }

    #region Success Scenarios

    [Test]
    public async Task Handle_WithValidDocument_ShouldCreateDocument()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateDocumentCommand
        {
            UserId = userId,
            DocType = "Passport",
            StoragePath = "/uploads/documents/passport_123.pdf",
            Verified = false
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "test@example.com" });

        Document? capturedDocument = null;
        _documentRepository
            .Setup(x => x.Insert(It.IsAny<Document>()))
            .Callback<Document>(d => capturedDocument = d);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedDocument!.UserId.Should().Be(userId);
        capturedDocument.DocType.Should().Be("Passport");
        capturedDocument.StoragePath.Should().Be("/uploads/documents/passport_123.pdf");
        capturedDocument.Verified.Should().BeFalse();
        capturedDocument.UploadedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        capturedDocument.Id.Should().Be(result);

        _documentRepository.Verify(x => x.Insert(It.IsAny<Document>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_ImmigrantPassportUpload_ShouldCreateUnverifiedDocument()
    {
        // Arrange - Business context: New immigrant uploads passport for verification
        var userId = Guid.NewGuid();
        var command = new CreateDocumentCommand
        {
            UserId = userId,
            DocType = "Passport",
            StoragePath = "/azure-blob/immigrants/documents/" + userId + "/passport.pdf",
            Verified = false // Documents need admin review
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = userId,
                Email = "newimmigrant@example.com",
                EmailVerified = true,
                Role = UserRole.User
            });

        Document? capturedDocument = null;
        _documentRepository
            .Setup(x => x.Insert(It.IsAny<Document>()))
            .Callback<Document>(d => capturedDocument = d);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedDocument!.DocType.Should().Be("Passport",
            "Passport is required for identity verification");
        capturedDocument.Verified.Should().BeFalse(
            "Uploaded documents require admin verification");
        capturedDocument.StoragePath.Should().Contain(userId.ToString(),
            "Documents should be organized by user ID for security");
    }

    [Test]
    public async Task Handle_DriversLicenseUpload_ShouldCreateDocument()
    {
        // Arrange - User uploads driver's license for car rental
        var userId = Guid.NewGuid();
        var command = new CreateDocumentCommand
        {
            UserId = userId,
            DocType = "DriversLicense",
            StoragePath = "/uploads/" + userId + "/drivers_license.pdf",
            Verified = false
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Role = UserRole.User });

        Document? capturedDocument = null;
        _documentRepository
            .Setup(x => x.Insert(It.IsAny<Document>()))
            .Callback<Document>(d => capturedDocument = d);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedDocument!.DocType.Should().Be("DriversLicense");
        capturedDocument.Id.Should().Be(result);
    }

    [Test]
    public async Task Handle_BankStatementUpload_ShouldCreateDocument()
    {
        // Arrange - Financial document for verification
        var userId = Guid.NewGuid();
        var command = new CreateDocumentCommand
        {
            UserId = userId,
            DocType = "BankStatement",
            StoragePath = "/uploads/financial/" + userId + "/statement.pdf",
            Verified = false
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        Document? capturedDocument = null;
        _documentRepository
            .Setup(x => x.Insert(It.IsAny<Document>()))
            .Callback<Document>(d => capturedDocument = d);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedDocument!.DocType.Should().Be("BankStatement");
    }

    #endregion

    #region Validation Scenarios

    [Test]
    public void Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateDocumentCommand
        {
            UserId = userId,
            DocType = "Passport",
            StoragePath = "/uploads/test.pdf"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("User");
        ex.Message.Should().Contain(userId.ToString());
        _documentRepository.Verify(x => x.Insert(It.IsAny<Document>()), Times.Never);
    }

    #endregion

    #region Error Scenarios

    [Test]
    public void Handle_InsertFails_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateDocumentCommand
        {
            UserId = userId,
            DocType = "Passport",
            StoragePath = "/uploads/test.pdf"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        _documentRepository
            .Setup(x => x.Insert(It.IsAny<Document>()))
            .Throws(new InvalidOperationException("Insert failed"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Be("Insert failed");
    }

    [Test]
    public void Handle_SaveChangesFails_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateDocumentCommand
        {
            UserId = userId,
            DocType = "Passport",
            StoragePath = "/uploads/test.pdf"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Be("Save failed");
        _documentRepository.Verify(x => x.Insert(It.IsAny<Document>()), Times.Once);
    }

    #endregion
}
