using AutoMapper;
using MigratingAssistant.Application.Common.Interfaces;
using Moq;

namespace MigratingAssistant.Application.UnitTests.Common;

/// <summary>
/// Base class for all unit tests providing common mocked dependencies
/// </summary>
public class SharedUnitTestBase
{
    #region Protected Properties

    protected Mock<IMapper> Mapper { get; }
    protected Mock<IUnitOfWork> UnitOfWork { get; }
    protected CancellationToken CancellationToken { get; }

    #endregion

    #region Constructor

    public SharedUnitTestBase()
    {
        Mapper = new Mock<IMapper>();
        UnitOfWork = new Mock<IUnitOfWork>();
        CancellationToken = new CancellationToken();

        // Setup default successful SaveChangesAsync behavior
        UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    #endregion

    #region Setup Methods

    /// <summary>
    /// Reset all mock setups - call this in [SetUp] if needed
    /// </summary>
    protected void ResetMocks()
    {
        Mapper.Reset();
        UnitOfWork.Reset();

        // Re-setup default behavior
        UnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    #endregion
}
