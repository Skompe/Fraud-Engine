using Capitec.FraudEngine.Application.Features.Investigations.ResolveFraudFlag;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Constants;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using Moq;

namespace Capitec.FraudEngine.Tests.Application.Features.Investigations;

public class ResolveFraudFlagHandlerTests
{
    private readonly Mock<IFraudFlagRepository> _flagRepoMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private readonly ResolveFraudFlagHandler _handler;

    public ResolveFraudFlagHandlerTests()
    {
        _handler = new ResolveFraudFlagHandler(
            _flagRepoMock.Object,
            _auditLogRepoMock.Object,
            _uowMock.Object);
    }

    private static FraudFlag PendingFlag() => new(
        transactionId: "TXN-001",
        customerId: "CUST-001",
        status: DomainConstants.FraudStatus.Pending,
        reason: "Velocity exceeded",
        severity: DomainConstants.Severity.High,
        source: "API",
        triggeredRules: ["HighVelocitySpend"]);

    [Fact]
    public async Task Handle_PendingFlag_ResolvesAndWritesAudit()
    {
        // Arrange
        var flag = PendingFlag();
        var flagId = flag.Id;

        _flagRepoMock
            .Setup(r => r.GetByIdAsync(flagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        AuditLog? capturedAudit = null;
        _auditLogRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>()))
            .Callback<AuditLog>(a => capturedAudit = a)
            .Returns(Task.CompletedTask);

        var command = new ResolveFraudFlagCommand(
            flagId,
            DomainConstants.FraudStatus.ConfirmedFraud,
            "Pattern matches known mule account.");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(DomainConstants.FraudStatus.ConfirmedFraud, flag.Status);
        Assert.NotNull(flag.ResolvedAt);

        Assert.NotNull(capturedAudit);
        Assert.Equal(AuditLog.Actions.FlagResolved, capturedAudit!.Action);
        Assert.Equal(DomainConstants.FraudStatus.Pending, capturedAudit.OldValue);
        Assert.Equal(DomainConstants.FraudStatus.ConfirmedFraud, capturedAudit.NewValue);
        Assert.Equal(flagId, capturedAudit.FraudFlagId);

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FlagNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _flagRepoMock
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FraudFlag?)null);

        // Act
        var result = await _handler.Handle(
            new ResolveFraudFlagCommand(missingId, DomainConstants.FraudStatus.FalsePositive, "notes"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyResolvedFlag_ThrowsInvalidOperation()
    {
        // Arrange
        var flag = PendingFlag();
        flag.Resolve(DomainConstants.FraudStatus.FalsePositive, "first resolution");

        _flagRepoMock
            .Setup(r => r.GetByIdAsync(flag.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(flag);

        var command = new ResolveFraudFlagCommand(
            flag.Id, DomainConstants.FraudStatus.ConfirmedFraud, "second attempt");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
