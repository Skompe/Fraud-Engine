using Capitec.FraudEngine.Application.Features.Transactions.ProcessSyncTransaction;
using Capitec.FraudEngine.Application.Constants;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.Domain.Constants;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Tests.Application.Features
{
    public class ProcessSyncTransactionHandlerTests
    {
        private readonly Mock<ITransactionRepository> _txnRepoMock = new();
        private readonly Mock<IFraudFlagRepository> _flagRepoMock = new();
        private readonly Mock<IRuleRepository> _ruleRepoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();
        private readonly Mock<IDynamicRuleEvaluator> _dynamicEvaluatorMock = new();
        private readonly Mock<IFraudRule> _builtInRuleMock = new();

        private readonly ProcessSyncTransactionHandler _handler;

        public ProcessSyncTransactionHandlerTests()
        {
            // Setup a single built-in rule
            var builtInRules = new List<IFraudRule> { _builtInRuleMock.Object };

            _handler = new ProcessSyncTransactionHandler(
                _txnRepoMock.Object,
                _flagRepoMock.Object,
                _ruleRepoMock.Object,
                _uowMock.Object,
                _dynamicEvaluatorMock.Object,
                builtInRules);
        }

        [Fact]
        public async Task Handle_WhenTransactionAlreadyExists_ReturnsConflictError()
        {
            // Arrange
            var command = new ProcessSyncTransactionCommand("TXN-DUP", "CUST-1", 100m, "ZAR");

            _txnRepoMock.Setup(r => r.ExistsAsync("TXN-DUP", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
            Assert.Equal(ErrorCodes.Transactions.DuplicateProcessedMessage, result.FirstError.Description);

            
            _txnRepoMock.Verify(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
            _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenNoRulesTriggered_ReturnsCleanStatus_AndSavesTransaction()
        {
            // Arrange
            var command = new ProcessSyncTransactionCommand("TXN-1", "CUST-1", 50m, "ZAR");

            _txnRepoMock.Setup(r => r.ExistsAsync("TXN-1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _ruleRepoMock.Setup(r => r.GetActiveRulesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<RuleConfiguration>());

            _dynamicEvaluatorMock.Setup(e => e.EvaluateAsync(It.IsAny<Transaction>(), It.IsAny<IEnumerable<RuleConfiguration>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>()); 

            _builtInRuleMock.Setup(r => r.EvaluateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null); 

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(DomainConstants.FraudStatus.Clean, result.Value.Status);
            Assert.Empty(result.Value.Rules);

            // Verify 
            _txnRepoMock.Verify(t => t.AddAsync(It.Is<Transaction>(tx => tx.TransactionId == "TXN-1"), It.IsAny<CancellationToken>()), Times.Once);
            _flagRepoMock.Verify(f => f.Add(It.IsAny<FraudFlag>()), Times.Never); 
            _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenOneRuleTriggered_CreatesMediumSeverityFlag()
        {
            // Arrange
            var command = new ProcessSyncTransactionCommand("TXN-2", "CUST-2", 150000m, "ZAR");

            _txnRepoMock.Setup(r => r.ExistsAsync("TXN-2", It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _ruleRepoMock.Setup(r => r.GetActiveRulesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<RuleConfiguration>());

            _dynamicEvaluatorMock.Setup(e => e.EvaluateAsync(It.IsAny<Transaction>(), It.IsAny<IEnumerable<RuleConfiguration>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "HighValueRule" }); 

            _builtInRuleMock.Setup(r => r.EvaluateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(DomainConstants.FraudStatus.Flagged, result.Value.Status);
            Assert.Single(result.Value.Rules);

            _flagRepoMock.Verify(f => f.Add(It.Is<FraudFlag>(flag =>
                flag.TransactionId == "TXN-2" &&
                flag.Severity == DomainConstants.Severity.Medium &&
                flag.Reason == "Triggered 1 fraud rules")), Times.Once);

            _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenMultipleRulesTriggered_CreatesHighSeverityFlag()
        {
            // Arrange
            var command = new ProcessSyncTransactionCommand("TXN-3", "CUST-3", 5000m, "USD");

            _txnRepoMock.Setup(r => r.ExistsAsync("TXN-3", It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _ruleRepoMock.Setup(r => r.GetActiveRulesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<RuleConfiguration>());

            _dynamicEvaluatorMock.Setup(e => e.EvaluateAsync(It.IsAny<Transaction>(), It.IsAny<IEnumerable<RuleConfiguration>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "ForeignCurrencyRule" }); 

            _builtInRuleMock.Setup(r => r.EvaluateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("HighVelocitySpend"); 

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(DomainConstants.FraudStatus.Flagged, result.Value.Status);
            Assert.Equal(2, result.Value.Rules.Count);


            //Verify
            _flagRepoMock.Verify(f => f.Add(It.Is<FraudFlag>(flag =>
                flag.TransactionId == "TXN-3" &&
                flag.Severity == DomainConstants.Severity.High &&
                flag.Reason == "Triggered 2 fraud rules")), Times.Once);

            _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
