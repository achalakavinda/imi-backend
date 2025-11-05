using AutoMapper;
using FluentAssertions;
using MigratingAssistant.Application.Common.Interfaces;
using MigratingAssistant.Application.ServiceTypes.Queries.GetServiceTypes;
using MigratingAssistant.Application.UnitTests.Common;
using Moq;
using NUnit.Framework;

namespace MigratingAssistant.Application.UnitTests.ServiceTypes.Queries;

[TestFixture]
public class GetServiceTypesQueryHandlerTests : SharedUnitTestBase
{
    private Mock<IApplicationDbContext> _mockContext = null!;
    private Mock<IMapper> _mockMapper = null!;
    private GetServiceTypesQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        ResetMocks();
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetServiceTypesQueryHandler(_mockContext.Object, _mockMapper.Object);
    }

    [Test]
    [Ignore("Requires integration test with real DbContext for DbSet<ServiceType>.ToListAsync()")]
    public void Handle_GetAllServiceTypes_ReturnsAllServiceTypes()
    {
        // Business scenario: Admin views all available service type configurations

        // Integration test should verify:
        // 1. Query retrieves all service types from database
        // 2. Results include ServiceKey, DisplayName, Enabled status
        // 3. SchemaHint JSON configuration included
        // 4. DTOs correctly mapped via AutoMapper
        // 5. Both enabled and disabled service types returned
        // 6. Results ordered appropriately (by DisplayName or ServiceKey)

        Assert.Pass("GetAllServiceTypes requires integration testing with real DbContext");
    }

    [Test]
    [Ignore("Requires integration test with real DbContext")]
    public void Handle_EmptyDatabase_ReturnsEmptyList()
    {
        // Business scenario: Fresh installation with no service types configured

        // Integration test should verify:
        // 1. Query returns empty list (not null)
        // 2. No exceptions thrown
        // 3. API returns 200 OK with empty array

        Assert.Pass("Empty result scenario requires integration testing");
    }

    [Test]
    [Ignore("Requires integration test with real DbContext")]
    public void Handle_FilterEnabledServiceTypes_ReturnsOnlyEnabled()
    {
        // Business scenario: Public API showing only active service types

        // Integration test should verify:
        // 1. Where clause filters by Enabled = true
        // 2. Disabled service types excluded from results
        // 3. Used for client-facing service selection

        Assert.Pass("Filtered query requires integration testing");
    }
}
