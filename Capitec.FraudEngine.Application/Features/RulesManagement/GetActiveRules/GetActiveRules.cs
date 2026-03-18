//using Capitec.FraudEngine.Application.Abstractions.Data;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.GetActiveRules
{
    public record GetActiveRulesQuery() : IRequest<ErrorOr<List<RuleResponse>>>;

    public record RuleResponse(string RuleName, string Description, string Expression);

    internal class GetActiveRulesHandler(IRuleRepository repository): IRequestHandler<GetActiveRulesQuery, ErrorOr<List<RuleResponse>>>
    {
        public async Task<ErrorOr<List<RuleResponse>>> Handle(GetActiveRulesQuery request, CancellationToken ct)
        {
            var rules = await repository.GetActiveRulesAsync(ct);

            return rules.Select(r => new RuleResponse(r.RuleName, r.Description, r.Expression!)).ToList();
        }
    }
}
