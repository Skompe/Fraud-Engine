using Capitec.FraudEngine.Application.Features.Investigations.IngestInvestigations;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Infrastructure.Messaging.Consumer;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Capitec.FraudEngine.Tests.Infrastructure.Messaging
{
    public class AsyncInvestigationConsumerTests
    {
        [Fact]
        public async Task Consume_MapsInvestigationFieldsToFraudFlagCorrectly()
        {
            // Arrange
            var item = new InvestigationItem(
                "TXN-100",
                "CUST-100",
                "ManualReview",
                "PENDING",
                "Suspicious transfer",
                "HIGH",
                Array.Empty<string>());

            var flagRepositoryMock = new Mock<IFraudFlagRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var loggerMock = new Mock<ILogger<AsyncInvestigationConsumer>>();
            var contextMock = new Mock<ConsumeContext<InvestigationItem>>();

            FraudFlag? addedFlag = null;
            flagRepositoryMock.Setup(r => r.Add(It.IsAny<FraudFlag>())).Callback<FraudFlag>(flag => addedFlag = flag);

            contextMock.SetupGet(c => c.Message).Returns(item);
            contextMock.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var consumer = new AsyncInvestigationConsumer(
                flagRepositoryMock.Object,
                unitOfWorkMock.Object,
                loggerMock.Object);

            // Act
            await consumer.Consume(contextMock.Object);

            // Assert
            Assert.NotNull(addedFlag);
            Assert.Equal(item.TransactionId, addedFlag!.TransactionId);
            Assert.Equal(item.CustomerId, addedFlag.CustomerId);
            Assert.Equal(item.Status, addedFlag.Status);
            Assert.Equal(item.Reason, addedFlag.Reason);
            Assert.Equal(item.Severity, addedFlag.Severity);
            Assert.Equal(item.Source, addedFlag.Source);

            unitOfWorkMock.Verify(u => u.SaveChangesAsync(CancellationToken.None), Times.Once);
        }
    }
}