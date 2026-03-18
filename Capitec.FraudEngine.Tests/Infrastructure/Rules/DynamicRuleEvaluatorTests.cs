using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capitec.FraudEngine.Infrastructure.Rules;

namespace Capitec.FraudEngine.Tests.Infrastructure.Rules
{
    public class DynamicRuleEvaluatorTests
    {
        private readonly Mock<ILogger<DynamicRuleEvaluator>> _loggerMock;
        private readonly DynamicRuleEvaluator _evaluator;

        public DynamicRuleEvaluatorTests()
        {
            _loggerMock = new Mock<ILogger<DynamicRuleEvaluator>>();
            _evaluator = new DynamicRuleEvaluator(_loggerMock.Object);
        }

        [Fact]
        public async Task EvaluateAsync_WithValidRule_ReturnsTriggeredRuleName()
        {
            // Arrange
            var transaction = new Transaction("TXN-1", "CUST-1", 75000m, "ZAR", DateTime.UtcNow);
            var rules = new List<RuleConfiguration>
        {
            new("HighValue", "Desc", "Amount > 50000")
        };

            // Act
            var result = await _evaluator.EvaluateAsync(transaction, rules);

            // Assert
            Assert.Single(result);
            Assert.Contains("HighValue", result);
        }

        [Fact]
        public async Task EvaluateAsync_WithMalformedRule_SafelySkipsAndContinues()
        {
            // Arrange
            var transaction = new Transaction("TXN-1", "CUST-1", 75000m, "ZAR", DateTime.UtcNow);
            var rules = new List<RuleConfiguration>
        {
            new("BadRule", "Desc", "tx.Amount > 50000"),
            new("GoodRule", "Desc", "Amount == 75000")
        };

            // Act
            var result = await _evaluator.EvaluateAsync(transaction, rules);

            // Assert
            Assert.Single(result);
            Assert.Contains("GoodRule", result); 
        }
    }
}
