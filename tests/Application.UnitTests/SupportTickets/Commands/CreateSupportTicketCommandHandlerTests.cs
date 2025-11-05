using FluentAssertions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.SupportTickets.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.SupportTickets.Commands;

[TestFixture]
public class CreateSupportTicketCommandHandlerTests : SharedUnitTestBase
{
    private CreateSupportTicketCommandHandler _handler = null!;
    private Mock<IRepository<SupportTicket>> _supportTicketRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _supportTicketRepository = new Mock<IRepository<SupportTicket>>();
        UnitOfWork.Setup(x => x.SupportTickets).Returns(_supportTicketRepository.Object);
        _handler = new CreateSupportTicketCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ImmigrantNeedsHelp_ShouldCreateTicket()
    {
        // Arrange - Business context: Immigrant needs help with booking issue
        var userId = Guid.NewGuid();
        var command = new CreateSupportTicketCommand
        {
            UserId = userId,
            Subject = "Issue with car rental booking payment",
            Body = "My payment was declined but I was charged. Please help!"
        };

        SupportTicket? capturedTicket = null;
        _supportTicketRepository
            .Setup(x => x.Insert(It.IsAny<SupportTicket>()))
            .Callback<SupportTicket>(t => capturedTicket = t);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedTicket.Should().NotBeNull();
        capturedTicket!.UserId.Should().Be(userId);
        capturedTicket.Subject.Should().Contain("booking payment");
        capturedTicket.Status.Should().Be(SupportTicketStatus.Open, "New tickets start as Open");
        capturedTicket.Id.Should().Be(result);
        _supportTicketRepository.Verify(x => x.Insert(It.IsAny<SupportTicket>()), Times.Once);
    }
}
