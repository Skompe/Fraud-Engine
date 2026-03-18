using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Transactions.IngestAsyncTransactions
{
    public record AsyncTransactionItem(string TransactionId, string CustomerId, decimal Amount, string Currency, DateTime Timestamp);

    public record IngestAsyncTransactionsCommand(List<AsyncTransactionItem> Transactions) : IRequest<ErrorOr<int>>;

    public class IngestAsyncTransactionsCommandHandler(IPublishEndpoint publishEndpoint) : IRequestHandler<IngestAsyncTransactionsCommand, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(IngestAsyncTransactionsCommand request, CancellationToken ct)
        {

            await publishEndpoint.PublishBatch(request.Transactions, ct);

            return request.Transactions.Count;
        }
    }
}
