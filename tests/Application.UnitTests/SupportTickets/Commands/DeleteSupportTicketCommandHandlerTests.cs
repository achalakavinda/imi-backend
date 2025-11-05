using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.SupportTickets.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.SupportTickets.Commands;

[TestFixture]
public class DeleteSupportTicketCommandHandlerTests : SharedUnitTestBase
{
    private Mock<IRepository<SupportTicket>> _mockSupportTicketRepository = null!;
    private DeleteSupportTicketCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _mockSupportTicketRepository = new Mock<IRepository<SupportTicket>>();
        UnitOfWork.Setup(u => u.SupportTickets).Returns(_mockSupportTicketRepository.Object);
        _handler = new DeleteSupportTicketCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidSupportTicketId_DeletesSupportTicketSuccessfully()
    {
        // Arrange - Business scenario: Delete a resolved support ticket
        var ticketId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var ticket = new SupportTicket
        {
            Id = ticketId,
            UserId = userId,
            Subject = "Account Access Issue",
            Body = "Cannot login to my account",
            Status = Domain.Enums.SupportTicketStatus.Resolved
        };

        _mockSupportTicketRepository
            .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new DeleteSupportTicketCommand(ticketId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockSupportTicketRepository.Verify(r => r.Delete(It.Is<SupportTicket>(t => t.Id == ticketId)), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_NonExistentSupportTicketId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _mockSupportTicketRepository
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportTicket)null!);

        var command = new DeleteSupportTicketCommand(nonExistentId);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken));

        ex.Message.Should().Contain(nameof(SupportTicket));
        ex.Message.Should().Contain(nonExistentId.ToString());

        _mockSupportTicketRepository.Verify(r => r.Delete(It.IsAny<SupportTicket>()), Times.Never);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_OpenTicket_AllowsDeletion()
    {
        // Arrange - Business scenario: User cancels their support request
        var ticketId = Guid.NewGuid();
        var ticket = new SupportTicket
        {
            Id = ticketId,
            UserId = Guid.NewGuid(),
            Subject = "Payment Issue",
            Body = "Payment not processed",
            Status = Domain.Enums.SupportTicketStatus.Open
        };

        _mockSupportTicketRepository
            .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new DeleteSupportTicketCommand(ticketId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockSupportTicketRepository.Verify(r => r.Delete(It.Is<SupportTicket>(t =>
            t.Id == ticketId &&
            t.Status == Domain.Enums.SupportTicketStatus.Open
        )), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ClosedTicket_AllowsDeletion()
    {
        // Arrange - Business scenario: Clean up old closed tickets
        var ticketId = Guid.NewGuid();
        var ticket = new SupportTicket
        {
            Id = ticketId,
            UserId = Guid.NewGuid(),
            Subject = "Document Verification",
            Body = "Need help with document upload",
            Status = Domain.Enums.SupportTicketStatus.Closed
        };

        _mockSupportTicketRepository
            .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new DeleteSupportTicketCommand(ticketId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockSupportTicketRepository.Verify(r => r.Delete(It.Is<SupportTicket>(t =>
            t.Id == ticketId &&
            t.Status == Domain.Enums.SupportTicketStatus.Closed
        )), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidCommand_UsesCancellationToken()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var ticket = new SupportTicket
        {
            Id = ticketId,
            UserId = Guid.NewGuid(),
            Subject = "General Inquiry",
            Body = "Question about services",
            Status = Domain.Enums.SupportTicketStatus.Resolved
        };

        _mockSupportTicketRepository
            .Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var command = new DeleteSupportTicketCommand(ticketId);
        var cancellationToken = new CancellationToken();

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockSupportTicketRepository.Verify(r => r.GetByIdAsync(ticketId, cancellationToken), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }
}
