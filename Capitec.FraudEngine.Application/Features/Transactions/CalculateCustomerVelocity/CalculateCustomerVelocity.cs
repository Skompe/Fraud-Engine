using Capitec.FraudEngine.Domain.Abstractions.Data;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Transactions.CalculateCustomerVelocity
{
    public record GetCustomerVelocityQuery(string CustomerId, int WindowInMinutes) : IRequest<ErrorOr<VelocityResponse>>;

    public record VelocityResponse(int TransactionCount, decimal TotalAmountSpent);


    public class CalculateCustomerVelocityHandler(ITransactionRepository repository): IRequestHandler<GetCustomerVelocityQuery, ErrorOr<VelocityResponse>>
    {
        public async Task<ErrorOr<VelocityResponse>> Handle(GetCustomerVelocityQuery request, CancellationToken ct)
        {
            var transactions = await repository.GetByCustomerIdAsync(request.CustomerId, ct);
            var cutoffTime = DateTime.UtcNow.AddMinutes(-request.WindowInMinutes);

            var recentTransactions = transactions.Where(t => t.Timestamp >= cutoffTime).ToList();

            return new VelocityResponse(recentTransactions.Count, recentTransactions.Sum(t => t.Amount));
        }
    }
}
