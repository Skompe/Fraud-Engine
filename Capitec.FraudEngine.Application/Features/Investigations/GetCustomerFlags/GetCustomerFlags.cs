using Capitec.FraudEngine.Domain.Abstractions.Data;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Investigations.GetCustomerFlags
{
    public record GetCustomerFlagsQuery(string CustomerId) : IRequest<ErrorOr<List<CustomerFlagResponse>>>;

    public record CustomerFlagResponse(
        Guid FlagId,
        string TransactionId,
        string Status,
        string Severity,
        IReadOnlyCollection<string> TriggeredRules,
        DateTime CreatedAt);

    internal class GetCustomerFlagsHandler(IFraudFlagRepository repository): IRequestHandler<GetCustomerFlagsQuery, ErrorOr<List<CustomerFlagResponse>>>
    {
        public async Task<ErrorOr<List<CustomerFlagResponse>>> Handle(GetCustomerFlagsQuery request, CancellationToken ct)
        {
            var flags = await repository.GetByCustomerIdAsync(request.CustomerId, ct);

            return flags.Select(f => new CustomerFlagResponse(
                            f.Id,
                            f.TransactionId,
                            f.Status,
                            f.Severity,
                            f.TriggeredRules,
                            f.CreatedAt))
                        .OrderByDescending(f => f.CreatedAt)
                        .ToList();
        }
    }
}
