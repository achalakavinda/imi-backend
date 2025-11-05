using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.SupportTickets.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.SupportTickets.Commands;

[TestFixture]
public class UpdateSupportTicketCommandHandlerTests : SharedUnitTestBase
{
    private UpdateSupportTicketCommandHandler _handler = null!;
    private Mock<IRepository<SupportTicket>> _supportTicketRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _supportTicketRepository = new Mock<IRepository<SupportTicket>>();
        UnitOfWork.Setup(x => x.SupportTickets).Returns(_supportTicketRepository.Object);
        _handler = new UpdateSupportTicketCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_AdminResolvesTicket_ShouldUpdateStatusToResolved()
    {
        // Arrange - Business context: Support admin resolves customer issue
        var ticketId = Guid.NewGuid();
        var existingTicket = new SupportTicket
        {
            Id = ticketId,
            UserId = Guid.NewGuid(),
            Subject = "Payment issue",
            Body = "Need help",
            Status = SupportTicketStatus.Open
        };

        var command = new UpdateSupportTicketCommand
        {
            Id = ticketId,
            Status = SupportTicketStatus.Resolved
        };

        _supportTicketRepository
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTicket);

        SupportTicket? updatedTicket = null;
        _supportTicketRepository
            .Setup(x => x.Update(It.IsAny<SupportTicket>()))
            .Callback<SupportTicket>(t => updatedTicket = t);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedTicket.Should().NotBeNull();
        updatedTicket!.Status.Should().Be(SupportTicketStatus.Resolved, "Admin can resolve tickets");
        _supportTicketRepository.Verify(x => x.Update(It.IsAny<SupportTicket>()), Times.Once);
    }

    [Test]
    public void Handle_TicketNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var command = new UpdateSupportTicketCommand { Id = ticketId, Status = SupportTicketStatus.Resolved };

        _supportTicketRepository
            .Setup(x => x.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportTicket?)null);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));
    }
}
