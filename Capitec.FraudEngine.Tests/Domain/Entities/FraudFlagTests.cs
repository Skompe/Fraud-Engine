using Capitec.FraudEngine.Domain.Constants;
using Capitec.FraudEngine.Domain.Entities;

namespace Capitec.FraudEngine.Tests.DomainModel.Entities
{
    public class FraudFlagTests
    {
        [Fact]
        public void Resolve_WhenPending_UpdatesResolutionFields()
        {
            // Arrange
            var flag = new FraudFlag(
                "TXN-1",
                "CUST-1",
                DomainConstants.FraudStatus.Pending,
                "Flagged for review",
                DomainConstants.Severity.Medium,
                DomainConstants.Source.RuleEngine,
                new[] { "HighVelocitySpend" });

            // Act
            flag.Resolve(DomainConstants.FraudStatus.FalsePositive, "Cleared after review");

            // Assert
            Assert.Equal(DomainConstants.FraudStatus.FalsePositive, flag.Status);
            Assert.Equal("Cleared after review", flag.AnalystNotes);
            Assert.NotNull(flag.ResolvedAt);
        }

        [Fact]
        public void Resolve_WhenNotPending_ThrowsInvalidOperationException()
        {
            // Arrange
            var flag = new FraudFlag(
                "TXN-2",
                "CUST-2",
                DomainConstants.FraudStatus.ConfirmedFraud,
                "Already confirmed",
                DomainConstants.Severity.High,
                DomainConstants.Source.RuleEngine,
                Array.Empty<string>());

            // Act
            var action = () => flag.Resolve(DomainConstants.FraudStatus.FalsePositive, "Should fail");

            // Assert
            Assert.Throws<InvalidOperationException>(action);
        }
    }
}