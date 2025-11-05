using FluentAssertions;
using MigratingAssistant.Application.Bookings.Commands;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.Bookings.Commands;

[TestFixture]
public class UpdateBookingCommandHandlerTests : SharedUnitTestBase
{
    private UpdateBookingCommandHandler _handler = null!;
    private Mock<IRepository<Booking>> _bookingRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _bookingRepository = new Mock<IRepository<Booking>>();
        UnitOfWork.Setup(x => x.Bookings).Returns(_bookingRepository.Object);
        _handler = new UpdateBookingCommandHandler(UnitOfWork.Object);
    }

    #region Success Scenarios

    [Test]
    public async Task Handle_WithFullUpdate_ShouldUpdateAllProperties()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var existingBooking = new Booking
        {
            Id = bookingId,
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            StartAt = DateTimeOffset.UtcNow,
            EndAt = DateTimeOffset.UtcNow.AddDays(7),
            Status = BookingStatus.Pending
        };

        var newStartAt = DateTimeOffset.UtcNow.AddDays(14);
        var newEndAt = DateTimeOffset.UtcNow.AddDays(21);

        var command = new UpdateBookingCommand
        {
            Id = bookingId,
            StartAt = newStartAt,
            EndAt = newEndAt,
            Status = BookingStatus.Confirmed
        };

        _bookingRepository
            .Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        Booking? updatedBooking = null;
        _bookingRepository
            .Setup(x => x.Update(It.IsAny<Booking>()))
            .Callback<Booking>(b => updatedBooking = b);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedBooking.Should().NotBeNull();
        updatedBooking!.StartAt.Should().Be(newStartAt);
        updatedBooking.EndAt.Should().Be(newEndAt);
        updatedBooking.Status.Should().Be(BookingStatus.Confirmed);
        _bookingRepository.Verify(x => x.Update(It.IsAny<Booking>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_WithPartialUpdate_StartAtOnly_ShouldUpdateOnlyStartAt()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var originalStartAt = DateTimeOffset.UtcNow;
        var originalEndAt = DateTimeOffset.UtcNow.AddDays(7);
        var originalStatus = BookingStatus.Pending;

        var existingBooking = new Booking
        {
            Id = bookingId,
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            StartAt = originalStartAt,
            EndAt = originalEndAt,
            Status = originalStatus
        };

        var newStartAt = DateTimeOffset.UtcNow.AddDays(14);

        var command = new UpdateBookingCommand
        {
            Id = bookingId,
            StartAt = newStartAt
        };

        _bookingRepository
            .Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        Booking? updatedBooking = null;
        _bookingRepository
            .Setup(x => x.Update(It.IsAny<Booking>()))
            .Callback<Booking>(b => updatedBooking = b);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedBooking.Should().NotBeNull();
        updatedBooking!.StartAt.Should().Be(newStartAt);
        updatedBooking.EndAt.Should().Be(originalEndAt);
        updatedBooking.Status.Should().Be(originalStatus);
    }

    [Test]
    public async Task Handle_ConfirmPendingBooking_ShouldUpdateStatusToConfirmed()
    {
        // Arrange - Business scenario: Payment completed, booking confirmed
        var bookingId = Guid.NewGuid();
        var existingBooking = new Booking
        {
            Id = bookingId,
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            StartAt = DateTimeOffset.UtcNow,
            EndAt = DateTimeOffset.UtcNow.AddDays(7),
            Status = BookingStatus.Pending
        };

        var command = new UpdateBookingCommand
        {
            Id = bookingId,
            Status = BookingStatus.Confirmed
        };

        _bookingRepository
            .Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        Booking? updatedBooking = null;
        _bookingRepository
            .Setup(x => x.Update(It.IsAny<Booking>()))
            .Callback<Booking>(b => updatedBooking = b);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedBooking.Should().NotBeNull();
        updatedBooking!.Status.Should().Be(BookingStatus.Confirmed,
            "Payment completion should confirm the booking");
        _bookingRepository.Verify(x => x.Update(It.IsAny<Booking>()), Times.Once);
    }

    [Test]
    public async Task Handle_CancelBooking_ShouldUpdateStatusToCancelled()
    {
        // Arrange - Business scenario: User cancels booking
        var bookingId = Guid.NewGuid();
        var existingBooking = new Booking
        {
            Id = bookingId,
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            StartAt = DateTimeOffset.UtcNow,
            EndAt = DateTimeOffset.UtcNow.AddDays(7),
            Status = BookingStatus.Confirmed
        };

        var command = new UpdateBookingCommand
        {
            Id = bookingId,
            Status = BookingStatus.Cancelled
        };

        _bookingRepository
            .Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        Booking? updatedBooking = null;
        _bookingRepository
            .Setup(x => x.Update(It.IsAny<Booking>()))
            .Callback<Booking>(b => updatedBooking = b);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedBooking.Should().NotBeNull();
        updatedBooking!.Status.Should().Be(BookingStatus.Cancelled,
            "User should be able to cancel their booking");
    }

    #endregion

    #region Error Scenarios

    [Test]
    public void Handle_BookingNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var command = new UpdateBookingCommand
        {
            Id = bookingId,
            Status = BookingStatus.Confirmed
        };

        _bookingRepository
            .Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("Booking");
        ex.Message.Should().Contain(bookingId.ToString());
        _bookingRepository.Verify(x => x.Update(It.IsAny<Booking>()), Times.Never);
        UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_SaveChangesFails_ShouldThrowException()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var existingBooking = new Booking
        {
            Id = bookingId,
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            StartAt = DateTimeOffset.UtcNow,
            EndAt = DateTimeOffset.UtcNow.AddDays(7),
            Status = BookingStatus.Pending
        };

        var command = new UpdateBookingCommand
        {
            Id = bookingId,
            Status = BookingStatus.Confirmed
        };

        _bookingRepository
            .Setup(x => x.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooking);

        UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Be("Database connection failed");
        _bookingRepository.Verify(x => x.Update(It.IsAny<Booking>()), Times.Once);
    }

    #endregion
}
