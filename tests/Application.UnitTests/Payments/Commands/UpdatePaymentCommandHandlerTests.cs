using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.Payments.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.Payments.Commands;

[TestFixture]
public class UpdatePaymentCommandHandlerTests : SharedUnitTestBase
{
    private UpdatePaymentCommandHandler _handler = null!;
    private Mock<IRepository<Payment>> _paymentRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _paymentRepository = new Mock<IRepository<Payment>>();
        UnitOfWork.Setup(x => x.Payments).Returns(_paymentRepository.Object);
        _handler = new UpdatePaymentCommandHandler(UnitOfWork.Object);
    }

    #region Success Scenarios

    [Test]
    public async Task Handle_UpdatePaymentStatus_ShouldUpdateStatus()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            UserId = Guid.NewGuid(),
            Amount = 800.00m,
            Currency = "AUD",
            Status = PaymentGatewayStatus.Initiated
        };

        var command = new UpdatePaymentCommand
        {
            Id = paymentId,
            Status = PaymentGatewayStatus.Success,
            GatewayReference = "stripe_pi_updated"
        };

        _paymentRepository
            .Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        Payment? updatedPayment = null;
        _paymentRepository
            .Setup(x => x.Update(It.IsAny<Payment>()))
            .Callback<Payment>(p => updatedPayment = p);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Status.Should().Be(PaymentGatewayStatus.Success);
        updatedPayment.GatewayReference.Should().Be("stripe_pi_updated");
        _paymentRepository.Verify(x => x.Update(It.IsAny<Payment>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_PaymentWebhookUpdate_ShouldUpdateFromPendingToSucceeded()
    {
        // Arrange - Business context: Payment gateway webhook confirms payment
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            UserId = Guid.NewGuid(),
            Amount = 1200.00m,
            Currency = "AUD",
            Status = PaymentGatewayStatus.Initiated,
            GatewayReference = "stripe_pi_pending"
        };

        var command = new UpdatePaymentCommand
        {
            Id = paymentId,
            Status = PaymentGatewayStatus.Success,
            GatewayReference = "stripe_pi_confirmed",
            Meta = "{\"webhookId\":\"evt_123\",\"confirmedAt\":\"2025-11-05T10:00:00Z\"}"
        };

        _paymentRepository
            .Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        Payment? updatedPayment = null;
        _paymentRepository
            .Setup(x => x.Update(It.IsAny<Payment>()))
            .Callback<Payment>(p => updatedPayment = p);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Status.Should().Be(PaymentGatewayStatus.Success,
            "Webhook should update payment status to succeeded");
        updatedPayment.Meta.Should().Contain("webhookId",
            "Webhook metadata should be stored for audit trail");
    }

    [Test]
    public async Task Handle_FailedPayment_ShouldUpdateToFailed()
    {
        // Arrange - Payment failed due to insufficient funds
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            UserId = Guid.NewGuid(),
            Amount = 800.00m,
            Currency = "AUD",
            Status = PaymentGatewayStatus.Initiated
        };

        var command = new UpdatePaymentCommand
        {
            Id = paymentId,
            Status = PaymentGatewayStatus.Failed,
            Meta = "{\"failureReason\":\"insufficient_funds\"}"
        };

        _paymentRepository
            .Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        Payment? updatedPayment = null;
        _paymentRepository
            .Setup(x => x.Update(It.IsAny<Payment>()))
            .Callback<Payment>(p => updatedPayment = p);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedPayment.Should().NotBeNull();
        updatedPayment!.Status.Should().Be(PaymentGatewayStatus.Failed);
        updatedPayment.Meta.Should().Contain("failureReason");
    }

    #endregion

    #region Error Scenarios

    [Test]
    public void Handle_PaymentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new UpdatePaymentCommand
        {
            Id = paymentId,
            Status = PaymentGatewayStatus.Success
        };

        _paymentRepository
            .Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("Payment");
        ex.Message.Should().Contain(paymentId.ToString());
        _paymentRepository.Verify(x => x.Update(It.IsAny<Payment>()), Times.Never);
    }

    [Test]
    public void Handle_SaveChangesFails_ShouldThrowException()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var existingPayment = new Payment
        {
            Id = paymentId,
            UserId = Guid.NewGuid(),
            Amount = 800.00m,
            Status = PaymentGatewayStatus.Initiated
        };

        var command = new UpdatePaymentCommand
        {
            Id = paymentId,
            Status = PaymentGatewayStatus.Success
        };

        _paymentRepository
            .Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Be("Database error");
        _paymentRepository.Verify(x => x.Update(It.IsAny<Payment>()), Times.Once);
    }

    #endregion
}
