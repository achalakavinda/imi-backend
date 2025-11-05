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
public class CreateBookingCommandHandlerTests : SharedUnitTestBase
{
    private CreateBookingCommandHandler _handler = null!;
    private Mock<IRepository<Booking>> _bookingRepository = null!;
    private Mock<IRepository<User>> _userRepository = null!;
    private Mock<IRepository<Listing>> _listingRepository = null!;
    private Mock<IRepository<InventoryItem>> _inventoryItemRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();

        _bookingRepository = new Mock<IRepository<Booking>>();
        _userRepository = new Mock<IRepository<User>>();
        _listingRepository = new Mock<IRepository<Listing>>();
        _inventoryItemRepository = new Mock<IRepository<InventoryItem>>();

        UnitOfWork.Setup(x => x.Bookings).Returns(_bookingRepository.Object);
        UnitOfWork.Setup(x => x.Users).Returns(_userRepository.Object);
        UnitOfWork.Setup(x => x.Listings).Returns(_listingRepository.Object);
        UnitOfWork.Setup(x => x.InventoryItems).Returns(_inventoryItemRepository.Object);

        _handler = new CreateBookingCommandHandler(UnitOfWork.Object);
    }

    #region Success Scenarios

    [Test]
    public async Task Handle_WithValidCarRentalBooking_ShouldCreateBookingSuccessfully()
    {
        // Arrange - Business scenario: Immigrant books a car rental
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var command = new CreateBookingCommand
        {
            UserId = userId,
            ListingId = listingId,
            InventoryItemId = inventoryItemId,
            StartAt = DateTimeOffset.UtcNow.AddDays(7),
            EndAt = DateTimeOffset.UtcNow.AddDays(14) // 7-day rental
        };

        // Mock existing user (immigrant)
        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "immigrant@example.com", Role = UserRole.User });

        // Mock existing listing (car rental)
        _listingRepository
            .Setup(x => x.GetByIdAsync(listingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Listing
            {
                Id = listingId,
                Title = "Toyota Camry - Weekly Rental",
                Status = ListingStatus.Active,
                Price = 350.00m
            });

        // Mock existing inventory item (specific car)
        _inventoryItemRepository
            .Setup(x => x.GetByIdAsync(inventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InventoryItem
            {
                Id = inventoryItemId,
                Sku = "CAR-001",
                Active = true
            });

        Booking? capturedBooking = null;
        _bookingRepository
            .Setup(x => x.Insert(It.IsAny<Booking>()))
            .Callback<Booking>(b => capturedBooking = b);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedBooking.Should().NotBeNull();
        capturedBooking!.UserId.Should().Be(userId);
        capturedBooking.ListingId.Should().Be(listingId);
        capturedBooking.InventoryItemId.Should().Be(inventoryItemId);
        capturedBooking.Status.Should().Be(BookingStatus.Pending);
        capturedBooking.StartAt.Should().Be(command.StartAt);
        capturedBooking.EndAt.Should().Be(command.EndAt);
        capturedBooking.Id.Should().Be(result); // Handler returns entity Id

        _userRepository.Verify(x => x.GetByIdAsync(userId, CancellationToken), Times.Once);
        _listingRepository.Verify(x => x.GetByIdAsync(listingId, CancellationToken), Times.Once);
        _inventoryItemRepository.Verify(x => x.GetByIdAsync(inventoryItemId, CancellationToken), Times.Once);
        _bookingRepository.Verify(x => x.Insert(It.IsAny<Booking>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_WithValidAccommodationBooking_ShouldCreateWithoutInventoryItem()
    {
        // Arrange - Business scenario: Immigrant books shared accommodation
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var command = new CreateBookingCommand
        {
            UserId = userId,
            ListingId = listingId,
            InventoryItemId = null, // Accommodation doesn't require specific inventory
            StartAt = DateTimeOffset.UtcNow.AddDays(30),
            EndAt = DateTimeOffset.UtcNow.AddMonths(6) // 6-month rental
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        _listingRepository
            .Setup(x => x.GetByIdAsync(listingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Listing
            {
                Id = listingId,
                Title = "Shared Apartment Near University",
                Status = ListingStatus.Active
            });

        Booking? capturedBooking = null;
        _bookingRepository
            .Setup(x => x.Insert(It.IsAny<Booking>()))
            .Callback<Booking>(b => capturedBooking = b);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedBooking.Should().NotBeNull();
        capturedBooking!.InventoryItemId.Should().BeNull();
        capturedBooking.Status.Should().Be(BookingStatus.Pending);
        capturedBooking.Id.Should().Be(result);

        _inventoryItemRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never); // Should not validate inventory item when null

        _bookingRepository.Verify(x => x.Insert(It.IsAny<Booking>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    #endregion

    #region Validation Scenarios

    [Test]
    public void Handle_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new CreateBookingCommand
        {
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            StartAt = DateTimeOffset.UtcNow.AddDays(1),
            EndAt = DateTimeOffset.UtcNow.AddDays(2)
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("User");
        ex.Message.Should().Contain(command.UserId.ToString());

        _bookingRepository.Verify(x => x.Insert(It.IsAny<Booking>()), Times.Never);
        UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_WhenListingNotFound_ShouldThrowNotFoundException()
    {
        // Arrange - Business scenario: User tries to book non-existent service
        var command = new CreateBookingCommand
        {
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            StartAt = DateTimeOffset.UtcNow.AddDays(1),
            EndAt = DateTimeOffset.UtcNow.AddDays(2)
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = command.UserId });

        _listingRepository
            .Setup(x => x.GetByIdAsync(command.ListingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Listing?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("Listing");
        ex.Message.Should().Contain(command.ListingId.ToString());

        _bookingRepository.Verify(x => x.Insert(It.IsAny<Booking>()), Times.Never);
        UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_WhenInventoryItemNotFound_ShouldThrowNotFoundException()
    {
        // Arrange - Business scenario: Specific car not available
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var inventoryItemId = Guid.NewGuid();

        var command = new CreateBookingCommand
        {
            UserId = userId,
            ListingId = listingId,
            InventoryItemId = inventoryItemId,
            StartAt = DateTimeOffset.UtcNow.AddDays(1),
            EndAt = DateTimeOffset.UtcNow.AddDays(2)
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        _listingRepository
            .Setup(x => x.GetByIdAsync(listingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Listing { Id = listingId });

        _inventoryItemRepository
            .Setup(x => x.GetByIdAsync(inventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("InventoryItem");
        ex.Message.Should().Contain(inventoryItemId.ToString());

        _bookingRepository.Verify(x => x.Insert(It.IsAny<Booking>()), Times.Never);
    }

    #endregion

    #region Error Scenarios

    [Test]
    public void Handle_WhenInsertFails_ShouldNotCallSaveChanges()
    {
        // Arrange
        var command = new CreateBookingCommand
        {
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            StartAt = DateTimeOffset.UtcNow.AddDays(1),
            EndAt = DateTimeOffset.UtcNow.AddDays(2)
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = command.UserId });

        _listingRepository
            .Setup(x => x.GetByIdAsync(command.ListingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Listing { Id = command.ListingId });

        _bookingRepository
            .Setup(x => x.Insert(It.IsAny<Booking>()))
            .Throws(new InvalidOperationException("Database error"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_WhenSaveChangesFails_ShouldThrowException()
    {
        // Arrange
        var command = new CreateBookingCommand
        {
            UserId = Guid.NewGuid(),
            ListingId = Guid.NewGuid(),
            StartAt = DateTimeOffset.UtcNow.AddDays(1),
            EndAt = DateTimeOffset.UtcNow.AddDays(2)
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = command.UserId });

        _listingRepository
            .Setup(x => x.GetByIdAsync(command.ListingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Listing { Id = command.ListingId });

        UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Be("Save failed");
        _bookingRepository.Verify(x => x.Insert(It.IsAny<Booking>()), Times.Once);
    }

    #endregion

    #region Business Scenario Tests

    [Test]
    public async Task Handle_ImmigrantFirstCarRental_ShouldCreatePendingBooking()
    {
        // Arrange - Business Context: New immigrant arriving in Australia needs car for university
        var userId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var command = new CreateBookingCommand
        {
            UserId = userId,
            ListingId = listingId,
            StartAt = DateTimeOffset.UtcNow.AddDays(14), // 2 weeks before arrival
            EndAt = DateTimeOffset.UtcNow.AddMonths(1) // 1 month rental
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = userId,
                Email = "newstudent@university.edu.au",
                EmailVerified = true,
                Role = UserRole.User
            });

        _listingRepository
            .Setup(x => x.GetByIdAsync(listingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Listing
            {
                Id = listingId,
                Title = "Budget Car Rental - Student Friendly",
                Description = "Affordable car rental for international students",
                Status = ListingStatus.Active,
                Price = 800.00m, // Monthly rate
                Currency = "AUD"
            });

        Booking? capturedBooking = null;
        _bookingRepository
            .Setup(x => x.Insert(It.IsAny<Booking>()))
            .Callback<Booking>(b => capturedBooking = b);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedBooking.Should().NotBeNull();
        capturedBooking!.Status.Should().Be(BookingStatus.Pending,
            "Booking should start as Pending and require payment confirmation");
        capturedBooking.UserId.Should().Be(userId);
        capturedBooking.ListingId.Should().Be(listingId);
        capturedBooking.StartAt.Should().BeCloseTo(command.StartAt, TimeSpan.FromSeconds(1));
        capturedBooking.EndAt.Should().BeCloseTo(command.EndAt, TimeSpan.FromSeconds(1));
        capturedBooking.Id.Should().Be(result);

        _bookingRepository.Verify(x => x.Insert(It.IsAny<Booking>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    #endregion
}
