using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.ServiceProviders.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using MigratingAssistant.Domain.Enums;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.ServiceProviders.Commands;

[TestFixture]
public class CreateServiceProviderCommandHandlerTests : SharedUnitTestBase
{
    private CreateServiceProviderCommandHandler _handler = null!;
    private Mock<IRepository<ServiceProvider>> _serviceProviderRepository = null!;
    private Mock<IRepository<User>> _userRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _serviceProviderRepository = new Mock<IRepository<ServiceProvider>>();
        _userRepository = new Mock<IRepository<User>>();
        UnitOfWork.Setup(x => x.ServiceProviders).Returns(_serviceProviderRepository.Object);
        UnitOfWork.Setup(x => x.Users).Returns(_userRepository.Object);
        _handler = new CreateServiceProviderCommandHandler(UnitOfWork.Object);
    }

    #region Success Scenarios

    [Test]
    public async Task Handle_WithValidProvider_ShouldCreateServiceProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateServiceProviderCommand
        {
            UserId = userId,
            ProviderName = "Budget Car Rentals",
            ProviderType = "CarRental",
            Verified = false,
            ProviderMetadata = "{\"abn\":\"12345678901\",\"location\":\"Sydney\"}"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "provider@example.com", Role = UserRole.Provider });

        ServiceProvider? capturedProvider = null;
        _serviceProviderRepository
            .Setup(x => x.Insert(It.IsAny<ServiceProvider>()))
            .Callback<ServiceProvider>(p => capturedProvider = p);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedProvider.Should().NotBeNull();
        capturedProvider!.UserId.Should().Be(userId);
        capturedProvider.ProviderName.Should().Be("Budget Car Rentals");
        capturedProvider.ProviderType.Should().Be("CarRental");
        capturedProvider.Verified.Should().BeFalse();
        capturedProvider.ProviderMetadata.Should().Contain("abn");
        capturedProvider.Id.Should().Be(result);

        _serviceProviderRepository.Verify(x => x.Insert(It.IsAny<ServiceProvider>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_CarRentalProviderOnboarding_ShouldCreateUnverifiedProvider()
    {
        // Arrange - Business context: New car rental company wants to list vehicles
        var userId = Guid.NewGuid();
        var command = new CreateServiceProviderCommand
        {
            UserId = userId,
            ProviderName = "Student Friendly Car Rentals",
            ProviderType = "CarRental",
            Verified = false, // Verification happens after admin review
            ProviderMetadata = "{\"abn\":\"98765432101\",\"businessAddress\":\"123 Main St, Sydney\",\"contactPhone\":\"+61400123456\"}"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = userId,
                Email = "carrentals@studentfriendly.com.au",
                EmailVerified = true,
                Role = UserRole.Provider
            });

        ServiceProvider? capturedProvider = null;
        _serviceProviderRepository
            .Setup(x => x.Insert(It.IsAny<ServiceProvider>()))
            .Callback<ServiceProvider>(p => capturedProvider = p);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedProvider.Should().NotBeNull();
        capturedProvider!.Verified.Should().BeFalse(
            "New providers require admin verification before listings go live");
        capturedProvider.ProviderType.Should().Be("CarRental");
        capturedProvider.ProviderMetadata.Should().Contain("abn",
            "Australian Business Number required for all providers");
        capturedProvider.ProviderMetadata.Should().Contain("businessAddress",
            "Physical address required for verification");
    }

    [Test]
    public async Task Handle_AccommodationProviderOnboarding_ShouldCreateProvider()
    {
        // Arrange - Business context: Property owner wants to rent to immigrants
        var userId = Guid.NewGuid();
        var command = new CreateServiceProviderCommand
        {
            UserId = userId,
            ProviderName = "Immigrant Welcome Apartments",
            ProviderType = "Accommodation",
            Verified = false,
            ProviderMetadata = "{\"propertyCount\":5,\"abn\":\"11122233344\",\"specialization\":\"StudentAccommodation\"}"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Id = userId,
                Email = "landlord@welcomeapts.com",
                Role = UserRole.Provider
            });

        ServiceProvider? capturedProvider = null;
        _serviceProviderRepository
            .Setup(x => x.Insert(It.IsAny<ServiceProvider>()))
            .Callback<ServiceProvider>(p => capturedProvider = p);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedProvider.Should().NotBeNull();
        capturedProvider!.ProviderType.Should().Be("Accommodation");
        capturedProvider.ProviderMetadata.Should().Contain("propertyCount");
    }

    [Test]
    public async Task Handle_VerifiedProvider_ShouldCreateWithVerifiedStatus()
    {
        // Arrange - Provider upgraded by admin
        var userId = Guid.NewGuid();
        var command = new CreateServiceProviderCommand
        {
            UserId = userId,
            ProviderName = "Premium Car Hire",
            ProviderType = "CarRental",
            Verified = true // Admin has verified
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Role = UserRole.Provider });

        ServiceProvider? capturedProvider = null;
        _serviceProviderRepository
            .Setup(x => x.Insert(It.IsAny<ServiceProvider>()))
            .Callback<ServiceProvider>(p => capturedProvider = p);

        // Act
        var result = await _handler.Handle(command, CancellationToken);

        // Assert
        capturedProvider.Should().NotBeNull();
        capturedProvider!.Verified.Should().BeTrue();
        capturedProvider.Id.Should().Be(result);
    }

    #endregion

    #region Validation Scenarios

    [Test]
    public void Handle_UserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateServiceProviderCommand
        {
            UserId = userId,
            ProviderName = "Test Provider",
            ProviderType = "CarRental"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("User");
        ex.Message.Should().Contain(userId.ToString());
        _serviceProviderRepository.Verify(x => x.Insert(It.IsAny<ServiceProvider>()), Times.Never);
    }

    #endregion

    #region Error Scenarios

    [Test]
    public void Handle_InsertFails_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateServiceProviderCommand
        {
            UserId = userId,
            ProviderName = "Test Provider",
            ProviderType = "CarRental"
        };

        _userRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId });

        _serviceProviderRepository
            .Setup(x => x.Insert(It.IsAny<ServiceProvider>()))
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
        var command = new CreateServiceProviderCommand
        {
            UserId = userId,
            ProviderName = "Test Provider",
            ProviderType = "CarRental"
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
        _serviceProviderRepository.Verify(x => x.Insert(It.IsAny<ServiceProvider>()), Times.Once);
    }

    #endregion
}
