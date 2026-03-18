using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using MassTransit.Transports;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace Capitec.FraudEngine.Application.Features.Investigations.IngestInvestigations
{
    public record InvestigationItem(string TransactionId, string CustomerId, string Source,string Status, string Reason, string Severity, IEnumerable<string> Rules);
    public record IngestInvestigationsCommand(List<InvestigationItem> Investigations) : IRequest<ErrorOr<int>>;

    public class IngestInvestigationsCommandHandler(IPublishEndpoint publishEndpoint) : IRequestHandler<IngestInvestigationsCommand, ErrorOr<int>>
    {
        public async Task<ErrorOr<int>> Handle(IngestInvestigationsCommand request, CancellationToken ct)
        {

            await publishEndpoint.PublishBatch(request.Investigations, ct);

            return request.Investigations.Count;
        }
    }
}
