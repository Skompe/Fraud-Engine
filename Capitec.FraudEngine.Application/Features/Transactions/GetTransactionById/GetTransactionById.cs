using Capitec.FraudEngine.Application.Features.Transactions.GetTransactionById;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Transactions.GetTransactionById
{
    public record GetTransactionByIdQuery(string TransactionId) : IRequest<ErrorOr<TransactionDetailsResponse>>;

    public record TransactionDetailsResponse(string TransactionId, string CustomerId, decimal Amount, string Currency, DateTime Timestamp);
}

internal class GetTransactionByIdHandler(ITransactionRepository repository): IRequestHandler<GetTransactionByIdQuery, ErrorOr<TransactionDetailsResponse>>
{
    public async Task<ErrorOr<TransactionDetailsResponse>> Handle(GetTransactionByIdQuery request, CancellationToken ct)
    {
        var tx = await repository.GetByIdAsync(request.TransactionId, ct);
        if (tx is null) return Error.NotFound("Transaction.NotFound", $"Transaction {request.TransactionId} not found.");

        return new TransactionDetailsResponse(tx.TransactionId, tx.CustomerId, tx.Amount, tx.Currency, tx.Timestamp);
    }
}