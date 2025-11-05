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
public class CreatePaymentCommandHandlerTests : SharedUnitTestBase
{
    private CreatePaymentCommandHandler _handler = null!;
    private Mock<IRepository<Payment>> _paymentRepository = null!;
    private Mock<IRepository<User>> _userRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _paymentRepository = new Mock<IRepository<Payment>>();
        _userRepository = new Mock<IRepository<User>>();
        UnitOfWork.Setup(x => x.Payments).Returns(_paymentRepository.Object);
        UnitOfWork.Setup(x => x.Users).Returns(_userRepository.Object);
        _handler = new CreatePaymentCommandHandler(UnitOfWork.Object);
    }

    #region Success Scenarios

    [Test]
    public async Task Handle_WithValidPayment_ShouldCreatePayment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreatePaymentCommand
        {
            UserId = userId,
            Amount = 800.00m,
            Currency = "AUD",
            GatewayReference = "stripe_pi_123456",
            Status = PaymentGatewayStatus.Success,
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "test@example.com" });

        Payment? capturedPayment = null;
        _paymentRepository
            .Setup(x => x.Insert(It.IsAny<Payment>()))
            .Callback<Payment>(p => capturedPayment = p);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedPayment.Should().NotBeNull();
        capturedPayment!.UserId.Should().Be(userId);
        capturedPayment.Amount.Should().Be(800.00m);
        capturedPayment.Currency.Should().Be("AUD");
        capturedPayment.GatewayReference.Should().Be("stripe_pi_123456");
        capturedPayment.Status.Should().Be(PaymentGatewayStatus.Success);
        capturedPayment.IdempotencyKey.Should().Be(command.IdempotencyKey);
        capturedPayment.Id.Should().Be(result);

        _paymentRepository.Verify(x => x.Insert(It.IsAny<Payment>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_BookingPaymentCompleted_ShouldCreateSuccessfulPayment()
    {
        // Arrange - Business context: User completes car rental booking payment
        var userId = Guid.NewGuid();
        var command = new CreatePaymentCommand
        {
            UserId = userId,
            Amount = 800.00m,
            Currency = "AUD",
            GatewayReference = "stripe_pi_car_rental_payment",
            Status = PaymentGatewayStatus.Success,
            Meta = "{\"bookingId\":\"" + Guid.NewGuid() + "\",\"listingType\":\"CarRental\"}",
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = userId,
                Email = "immigrant@example.com",
                Role = UserRole.User
            });

        Payment? capturedPayment = null;
        _paymentRepository
            .Setup(x => x.Insert(It.IsAny<Payment>()))
            .Callback<Payment>(p => capturedPayment = p);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedPayment.Should().NotBeNull();
        capturedPayment!.Status.Should().Be(PaymentGatewayStatus.Success,
            "Payment should be marked as succeeded after gateway confirmation");
        capturedPayment.GatewayReference.Should().NotBeNullOrEmpty(
            "Gateway reference is required for tracking");
        capturedPayment.IdempotencyKey.Should().NotBeNullOrEmpty(
            "Idempotency key prevents duplicate charges");
        capturedPayment.Meta.Should().Contain("bookingId",
            "Payment metadata should link to the booking");
    }

    [Test]
    public async Task Handle_PendingPayment_ShouldCreatePendingPayment()
    {
        // Arrange - Payment initiated but not yet completed
        var userId = Guid.NewGuid();
        var command = new CreatePaymentCommand
        {
            UserId = userId,
            Amount = 1200.00m,
            Currency = "AUD",
            Status = PaymentGatewayStatus.Initiated,
            IdempotencyKey = Guid.NewGuid().ToString()
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        Payment? capturedPayment = null;
        _paymentRepository
            .Setup(x => x.Insert(It.IsAny<Payment>()))
            .Callback<Payment>(p => capturedPayment = p);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedPayment.Should().NotBeNull();
        capturedPayment!.Status.Should().Be(PaymentGatewayStatus.Initiated);
        capturedPayment.Id.Should().Be(result);
    }

    #endregion

    #region Validation Scenarios

    [Test]
    public void Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreatePaymentCommand
        {
            UserId = userId,
            Amount = 800.00m,
            Currency = "AUD",
            Status = PaymentGatewayStatus.Success
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("User");
        ex.Message.Should().Contain(userId.ToString());
        _paymentRepository.Verify(x => x.Insert(It.IsAny<Payment>()), Times.Never);
    }

    #endregion

    #region Error Scenarios

    [Test]
    public void Handle_InsertFails_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreatePaymentCommand
        {
            UserId = userId,
            Amount = 800.00m,
            Currency = "AUD",
            Status = PaymentGatewayStatus.Success
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        _paymentRepository
            .Setup(x => x.Insert(It.IsAny<Payment>()))
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
        var command = new CreatePaymentCommand
        {
            UserId = userId,
            Amount = 800.00m,
            Currency = "AUD",
            Status = PaymentGatewayStatus.Success
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
        _paymentRepository.Verify(x => x.Insert(It.IsAny<Payment>()), Times.Once);
    }

    #endregion
}
