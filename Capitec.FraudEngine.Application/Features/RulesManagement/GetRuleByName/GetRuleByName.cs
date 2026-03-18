using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.GetRuleByName
{
    public record GetRuleByNameQuery(string RuleName) : IRequest<RuleConfiguration?>;
    public class GetRuleByNameQueryHandler(IRuleRepository ruleRepository) : IRequestHandler<GetRuleByNameQuery, RuleConfiguration?>
    {
        public async Task<RuleConfiguration?> Handle(GetRuleByNameQuery request, CancellationToken cancellationToken)
        {
            return await ruleRepository.GetByNameAsync(request.RuleName, cancellationToken);
        }
    }
}
