using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.Documents.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.Documents.Commands;

[TestFixture]
public class DeleteDocumentCommandHandlerTests : SharedUnitTestBase
{
    private Mock<IRepository<Document>> _mockDocumentRepository = null!;
    private DeleteDocumentCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _mockDocumentRepository = new Mock<IRepository<Document>>();
        UnitOfWork.Setup(u => u.Documents).Returns(_mockDocumentRepository.Object);
        _handler = new DeleteDocumentCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidDocumentId_DeletesDocumentSuccessfully()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var document = new Document
        {
            Id = documentId,
            UserId = userId,
            DocType = "Passport",
            StoragePath = "/uploads/documents/passport_12345.pdf",
            Verified = true,
            UploadedAt = DateTimeOffset.UtcNow.AddDays(-5)
        };

        _mockDocumentRepository
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new DeleteDocumentCommand(documentId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockDocumentRepository.Verify(r => r.Delete(It.Is<Document>(d => d.Id == documentId)), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_NonExistentDocumentId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _mockDocumentRepository
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document)null!);

        var command = new DeleteDocumentCommand(nonExistentId);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        ex.Message.Should().Contain(nameof(Document));
        ex.Message.Should().Contain(nonExistentId.ToString());

        _mockDocumentRepository.Verify(r => r.Delete(It.IsAny<Document>()), Times.Never);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_UnverifiedDocument_AllowsDeletion()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document
        {
            Id = documentId,
            UserId = Guid.NewGuid(),
            DocType = "Visa",
            StoragePath = "/uploads/documents/visa_67890.pdf",
            Verified = false,
            UploadedAt = DateTimeOffset.UtcNow
        };

        _mockDocumentRepository
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new DeleteDocumentCommand(documentId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockDocumentRepository.Verify(r => r.Delete(It.Is<Document>(d =>
            d.Id == documentId &&
            d.Verified == false
        )), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_VerifiedDocument_AllowsDeletion()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document
        {
            Id = documentId,
            UserId = Guid.NewGuid(),
            DocType = "Work Permit",
            StoragePath = "/uploads/documents/work_permit_22222.pdf",
            Verified = true,
            UploadedAt = DateTimeOffset.UtcNow.AddMonths(-1)
        };

        _mockDocumentRepository
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new DeleteDocumentCommand(documentId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockDocumentRepository.Verify(r => r.Delete(It.Is<Document>(d =>
            d.Id == documentId &&
            d.Verified == true &&
            d.DocType == "Work Permit"
        )), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidCommand_UsesCancellationToken()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document
        {
            Id = documentId,
            UserId = Guid.NewGuid(),
            DocType = "Passport",
            StoragePath = "/uploads/documents/test.pdf",
            Verified = false,
            UploadedAt = DateTimeOffset.UtcNow
        };

        _mockDocumentRepository
            .Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new DeleteDocumentCommand(documentId);
        var cancellationToken = new CancellationToken();

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockDocumentRepository.Verify(r => r.GetByIdAsync(documentId, cancellationToken), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }
}
