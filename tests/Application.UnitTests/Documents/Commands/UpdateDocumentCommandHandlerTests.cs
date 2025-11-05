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
public class UpdateDocumentCommandHandlerTests : SharedUnitTestBase
{
    private UpdateDocumentCommandHandler _handler = null!;
    private Mock<IRepository<Document>> _documentRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _documentRepository = new Mock<IRepository<Document>>();
        UnitOfWork.Setup(x => x.Documents).Returns(_documentRepository.Object);
        _handler = new UpdateDocumentCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_VerifyDocument_ShouldUpdateVerificationStatus()
    {
        // Arrange - Admin verifies uploaded passport
        var documentId = Guid.NewGuid();
        var existingDocument = new Document
        {
            Id = documentId,
            UserId = Guid.NewGuid(),
            DocType = "Passport",
            StoragePath = "/uploads/passport.pdf",
            Verified = false
        };

        var command = new UpdateDocumentCommand
        {
            Id = documentId,
            Verified = true
        };

        _documentRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDocument);

        Document? updatedDocument = null;
        _documentRepository
            .Setup(x => x.Update(It.IsAny<Document>()))
            .Callback<Document>(d => updatedDocument = d);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedDocument.Should().NotBeNull();
        updatedDocument!.Verified.Should().BeTrue("Admin verification enables user access to services");
        _documentRepository.Verify(x => x.Update(It.IsAny<Document>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public void Handle_DocumentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var command = new UpdateDocumentCommand { Id = documentId, Verified = true };

        _documentRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("Document");
        _documentRepository.Verify(x => x.Update(It.IsAny<Document>()), Times.Never);
    }
}
