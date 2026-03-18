using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Application.Constants;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Transactions.IngestAsyncTransaction
{
    public record IngestAsyncTransactionCommand(string TransactionId, string CustomerId, decimal Amount, string Currency)
       : IRequest<ErrorOr<Success>>;

    internal class IngestAsyncTransactionHandler(ITransactionRepository repository,IUnitOfWork unitOfWork): IRequestHandler<IngestAsyncTransactionCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(IngestAsyncTransactionCommand request, CancellationToken ct)
        {
            if (await repository.ExistsAsync(request.TransactionId, ct))
                return Error.Conflict(ErrorCodes.Transactions.DuplicateCode, ErrorCodes.Transactions.DuplicateIngestedMessage);

            var tx = new Transaction(request.TransactionId, request.CustomerId, request.Amount, request.Currency, DateTime.UtcNow);

            await repository.AddAsync(tx, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
