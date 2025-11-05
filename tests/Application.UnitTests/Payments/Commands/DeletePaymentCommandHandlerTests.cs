using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.Payments.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.Payments.Commands;

[TestFixture]
public class DeletePaymentCommandHandlerTests : SharedUnitTestBase
{
    private Mock<IRepository<Payment>> _mockPaymentRepository = null!;
    private DeletePaymentCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _mockPaymentRepository = new Mock<IRepository<Payment>>();
        UnitOfWork.Setup(u => u.Payments).Returns(_mockPaymentRepository.Object);
        _handler = new DeletePaymentCommandHandler(UnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ValidPaymentId_DeletesPaymentSuccessfully()
    {
        // Arrange - Business scenario: Delete a failed payment record
        var paymentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var payment = new Payment
        {
            Id = paymentId,
            UserId = userId,
            Amount = 500.00m,
            Currency = "AUD",
            Status = Domain.Enums.PaymentGatewayStatus.Failed
        };

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var command = new DeletePaymentCommand(paymentId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockPaymentRepository.Verify(r => r.Delete(It.Is<Payment>(p => p.Id == paymentId)), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_NonExistentPaymentId_ThrowsNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment)null!);

        var command = new DeletePaymentCommand(nonExistentId);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken));

        ex.Message.Should().Contain(nameof(Payment));
        ex.Message.Should().Contain(nonExistentId.ToString());

        _mockPaymentRepository.Verify(r => r.Delete(It.IsAny<Payment>()), Times.Never);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_PendingPayment_AllowsDeletion()
    {
        // Arrange - Business scenario: Cancel a pending payment
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            UserId = Guid.NewGuid(),
            Amount = 1200.00m,
            Currency = "AUD",
            Status = Domain.Enums.PaymentGatewayStatus.Initiated
        };

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var command = new DeletePaymentCommand(paymentId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockPaymentRepository.Verify(r => r.Delete(It.Is<Payment>(p =>
            p.Id == paymentId &&
            p.Status == Domain.Enums.PaymentGatewayStatus.Initiated
        )), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_SuccessfulPayment_AllowsDeletion()
    {
        // Arrange - Business scenario: Archive old successful payment
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            UserId = Guid.NewGuid(),
            Amount = 800.00m,
            Currency = "AUD",
            Status = Domain.Enums.PaymentGatewayStatus.Success
        };

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var command = new DeletePaymentCommand(paymentId);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        _mockPaymentRepository.Verify(r => r.Delete(It.Is<Payment>(p =>
            p.Id == paymentId &&
            p.Status == Domain.Enums.PaymentGatewayStatus.Success
        )), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidCommand_UsesCancellationToken()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            UserId = Guid.NewGuid(),
            Amount = 300.00m,
            Currency = "AUD",
            Status = Domain.Enums.PaymentGatewayStatus.Failed
        };

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var command = new DeletePaymentCommand(paymentId);
        var cancellationToken = new CancellationToken();

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockPaymentRepository.Verify(r => r.GetByIdAsync(paymentId, cancellationToken), Times.Once);
        UnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }
}
