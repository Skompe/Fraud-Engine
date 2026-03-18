using Capitec.FraudEngine.Application.Features.RulesManagement.GetActiveRules;
using Capitec.FraudEngine.Application.Features.RulesManagement.GetRuleByName;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using Moq;

namespace Capitec.FraudEngine.Tests.Application.Features.RulesManagement;

public class RuleQueryHandlerTests
{
    private readonly Mock<IRuleRepository> _ruleRepoMock = new();

    private static RuleConfiguration Rule(string name, string expr = "amount > 1000") =>
        new(name, $"Rule {name}", expr);

    [Fact]
    public async Task GetActiveRules_ReturnsAllActiveRules()
    {
        // Arrange
        var rules = new List<RuleConfiguration>
        {
            Rule("HighAmount", "amount > 5000"),
            Rule("HighVelocity", "count > 10")
        };
        _ruleRepoMock
            .Setup(r => r.GetActiveRulesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        var handler = new GetActiveRulesHandler(_ruleRepoMock.Object);

        // Act
        var result = await handler.Handle(new GetActiveRulesQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count);
        Assert.Contains(result.Value, r => r.RuleName == "HighAmount");
        Assert.Contains(result.Value, r => r.RuleName == "HighVelocity");
    }

    [Fact]
    public async Task GetActiveRules_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _ruleRepoMock
            .Setup(r => r.GetActiveRulesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RuleConfiguration>());

        var handler = new GetActiveRulesHandler(_ruleRepoMock.Object);

        // Act
        var result = await handler.Handle(new GetActiveRulesQuery(), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetActiveRules_MapsRuleNameDescriptionAndExpression()
    {
        // Arrange
        var rule = Rule("VelocityCheck", "count > 5");
        _ruleRepoMock
            .Setup(r => r.GetActiveRulesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([rule]);

        var handler = new GetActiveRulesHandler(_ruleRepoMock.Object);

        // Act
        var result = await handler.Handle(new GetActiveRulesQuery(), CancellationToken.None);

        // Assert
        var response = Assert.Single(result.Value);
        Assert.Equal("VelocityCheck", response.RuleName);
        Assert.Equal("Rule VelocityCheck", response.Description);
        Assert.Equal("count > 5", response.Expression);
    }


    [Fact]
    public async Task GetRuleByName_ExistingRule_ReturnsRule()
    {
        // Arrange
        var rule = Rule("AmountCheck");
        _ruleRepoMock
            .Setup(r => r.GetByNameAsync("AmountCheck", It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        var handler = new GetRuleByNameQueryHandler(_ruleRepoMock.Object);

        // Act
        var result = await handler.Handle(
            new GetRuleByNameQuery("AmountCheck"),
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AmountCheck", result!.RuleName);
    }

    [Fact]
    public async Task GetRuleByName_UnknownRule_ReturnsNull()
    {
        // Arrange
        _ruleRepoMock
            .Setup(r => r.GetByNameAsync("Unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RuleConfiguration?)null);

        var handler = new GetRuleByNameQueryHandler(_ruleRepoMock.Object);

        // Act
        var result = await handler.Handle(
            new GetRuleByNameQuery("Unknown"),
            CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
