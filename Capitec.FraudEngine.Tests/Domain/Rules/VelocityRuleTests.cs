using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Constants;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Domain.Rules;
using Moq;

namespace Capitec.FraudEngine.Tests.Domain.Rules;

public class VelocityRuleTests
{
    private readonly Mock<ITransactionRepository> _txnRepoMock = new();
    private readonly VelocityRule _rule;

    public VelocityRuleTests()
    {
        _rule = new VelocityRule(_txnRepoMock.Object);
    }

    private static Transaction SampleTransaction(string custId = "CUST-001") =>
        new("TXN-001", custId, 100m, "ZAR", DateTime.UtcNow);

    [Fact]
    public void RuleKey_ReturnsHighVelocitySpend()
    {
        Assert.Equal("HighVelocitySpend", _rule.RuleKey);
    }

    [Fact]
    public async Task EvaluateAsync_WhenCountAtThreshold_ReturnsRuleKey()
    {
        var txn = SampleTransaction();
        _txnRepoMock
            .Setup(r => r.GetTransactionCountAsync(txn.CustomerId, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var result = await _rule.EvaluateAsync(txn, CancellationToken.None);

        Assert.Equal("HighVelocitySpend", result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenCountExceedsThreshold_ReturnsRuleKey()
    {
        var txn = SampleTransaction();
        _txnRepoMock
            .Setup(r => r.GetTransactionCountAsync(txn.CustomerId, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var result = await _rule.EvaluateAsync(txn, CancellationToken.None);

        Assert.Equal("HighVelocitySpend", result);
    }

    [Fact]
    public async Task EvaluateAsync_WhenCountBelowThreshold_ReturnsNull()
    {
        var txn = SampleTransaction();
        _txnRepoMock
            .Setup(r => r.GetTransactionCountAsync(txn.CustomerId, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);

        var result = await _rule.EvaluateAsync(txn, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_AfterUpdateParameters_UsesNewThreshold()
    {
        // Arrange
        _rule.UpdateParameters("""{"MaxTransactions":3,"TimeWindowMinutes":10}""");

        var txn = SampleTransaction();
        _txnRepoMock
            .Setup(r => r.GetTransactionCountAsync(txn.CustomerId, TimeSpan.FromMinutes(10), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _rule.EvaluateAsync(txn, CancellationToken.None);

        // Assert
        Assert.Equal("HighVelocitySpend", result);
    }

    [Fact]
    public async Task EvaluateAsync_AfterUpdateParameters_BelowNewThreshold_ReturnsNull()
    {
        _rule.UpdateParameters("""{"MaxTransactions":3}""");

        var txn = SampleTransaction();
        _txnRepoMock
            .Setup(r => r.GetTransactionCountAsync(txn.CustomerId, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var result = await _rule.EvaluateAsync(txn, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_UpdateParameters_EmptyJson_UsesDefaultThreshold()
    {
        // Should not crash and retain defaults
        _rule.UpdateParameters(string.Empty);

        var txn = SampleTransaction();
        _txnRepoMock
            .Setup(r => r.GetTransactionCountAsync(txn.CustomerId, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(6);

        var result = await _rule.EvaluateAsync(txn, CancellationToken.None);

        Assert.Equal("HighVelocitySpend", result);
    }
}
