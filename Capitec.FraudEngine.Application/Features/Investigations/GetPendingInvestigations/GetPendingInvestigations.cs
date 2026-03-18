using Capitec.FraudEngine.Domain.Abstractions.Data;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Investigations.GetPendingInvestigations
{
    public record GetPendingInvestigationsQuery() : IRequest<ErrorOr<List<FraudFlagResponse>>>;

    public record FraudFlagResponse(Guid FlagId, string TransactionId, string Severity, DateTime CreatedAt);

    internal class GetPendingInvestigationsHandler(IFraudFlagRepository repository): IRequestHandler<GetPendingInvestigationsQuery, ErrorOr<List<FraudFlagResponse>>>
    {
        public async Task<ErrorOr<List<FraudFlagResponse>>> Handle(GetPendingInvestigationsQuery request, CancellationToken ct)
        {
            var pendingFlags = await repository.GetPendingFlagsAsync(ct);
            return pendingFlags.Select(f => new FraudFlagResponse(f.Id, f.TransactionId, f.Severity, f.CreatedAt)).ToList();
        }
    }
}
