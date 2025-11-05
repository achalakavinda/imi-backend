using FluentAssertions;
using MigratingAssistant.Application.Common.Exceptions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.ServiceProviders.Commands;
using MigratingAssistant.Application.UnitTests.Common;
using MigratingAssistant.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.ServiceProviders.Commands;

[TestFixture]
public class UpdateServiceProviderCommandHandlerTests : SharedUnitTestBase
{
    private UpdateServiceProviderCommandHandler _handler = null!;
    private Mock<IRepository<ServiceProvider>> _serviceProviderRepository = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _serviceProviderRepository = new Mock<IRepository<ServiceProvider>>();
        UnitOfWork.Setup(x => x.ServiceProviders).Returns(_serviceProviderRepository.Object);
        _handler = new UpdateServiceProviderCommandHandler(UnitOfWork.Object);
    }

    #region Success Scenarios

    [Test]
    public async Task Handle_UpdateProviderDetails_ShouldUpdateProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var existingProvider = new ServiceProvider
        {
            Id = providerId,
            UserId = Guid.NewGuid(),
            ProviderName = "Old Name",
            ProviderType = "CarRental",
            Verified = false
        };

        var command = new UpdateServiceProviderCommand
        {
            Id = providerId,
            ProviderName = "Updated Car Rentals",
            ProviderMetadata = "{\"updated\":true}"
        };

        _serviceProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        ServiceProvider? updatedProvider = null;
        _serviceProviderRepository
            .Setup(x => x.Update(It.IsAny<ServiceProvider>()))
            .Callback<ServiceProvider>(p => updatedProvider = p);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedProvider.Should().NotBeNull();
        updatedProvider!.ProviderName.Should().Be("Updated Car Rentals");
        updatedProvider.ProviderMetadata.Should().Contain("updated");
        _serviceProviderRepository.Verify(x => x.Update(It.IsAny<ServiceProvider>()), Times.Once);
        UnitOfWork.Verify(x => x.SaveChangesAsync(CancellationToken), Times.Once);
    }

    [Test]
    public async Task Handle_VerifyProvider_ShouldUpdateVerificationStatus()
    {
        // Arrange - Business context: Admin verifies provider after document review
        var providerId = Guid.NewGuid();
        var existingProvider = new ServiceProvider
        {
            Id = providerId,
            UserId = Guid.NewGuid(),
            ProviderName = "Budget Car Rentals",
            ProviderType = "CarRental",
            Verified = false
        };

        var command = new UpdateServiceProviderCommand
        {
            Id = providerId,
            Verified = true,
            ProviderMetadata = "{\"verifiedBy\":\"admin@platform.com\",\"verifiedAt\":\"2025-11-05T10:00:00Z\"}"
        };

        _serviceProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        ServiceProvider? updatedProvider = null;
        _serviceProviderRepository
            .Setup(x => x.Update(It.IsAny<ServiceProvider>()))
            .Callback<ServiceProvider>(p => updatedProvider = p);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedProvider.Should().NotBeNull();
        updatedProvider!.Verified.Should().BeTrue(
            "Admin verification allows provider listings to go live");
        updatedProvider.ProviderMetadata.Should().Contain("verifiedBy",
            "Audit trail for who verified the provider");
    }

    [Test]
    public async Task Handle_UpdateProviderType_ShouldUpdateType()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var existingProvider = new ServiceProvider
        {
            Id = providerId,
            UserId = Guid.NewGuid(),
            ProviderName = "Multi Service Provider",
            ProviderType = "CarRental",
            Verified = true
        };

        var command = new UpdateServiceProviderCommand
        {
            Id = providerId,
            ProviderType = "AccommodationAndCarRental"
        };

        _serviceProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        ServiceProvider? updatedProvider = null;
        _serviceProviderRepository
            .Setup(x => x.Update(It.IsAny<ServiceProvider>()))
            .Callback<ServiceProvider>(p => updatedProvider = p);

        // Act
        await _handler.Handle(command, CancellationToken);

        // Assert
        updatedProvider.Should().NotBeNull();
        updatedProvider!.ProviderType.Should().Be("AccommodationAndCarRental");
    }

    #endregion

    #region Error Scenarios

    [Test]
    public void Handle_ProviderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UpdateServiceProviderCommand
        {
            Id = providerId,
            ProviderName = "Updated Name"
        };

        _serviceProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceProvider?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Contain("ServiceProvider");
        ex.Message.Should().Contain(providerId.ToString());
        _serviceProviderRepository.Verify(x => x.Update(It.IsAny<ServiceProvider>()), Times.Never);
    }

    [Test]
    public void Handle_SaveChangesFails_ShouldThrowException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var existingProvider = new ServiceProvider
        {
            Id = providerId,
            UserId = Guid.NewGuid(),
            ProviderName = "Test Provider",
            ProviderType = "CarRental"
        };

        var command = new UpdateServiceProviderCommand
        {
            Id = providerId,
            Verified = true
        };

        _serviceProviderRepository
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProvider);

        UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken));

        ex!.Message.Should().Be("Database error");
        _serviceProviderRepository.Verify(x => x.Update(It.IsAny<ServiceProvider>()), Times.Once);
    }

    #endregion
}
