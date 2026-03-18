using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.UpdateRule
{
    public record UpdateRuleCommand(string RuleName,string Description,string? Expression,string? Parameters,bool IsActive) : IRequest<ErrorOr<RuleConfiguration?>>;
    public class UpdateRuleCommandHandler(IRuleRepository ruleRepository): IRequestHandler<UpdateRuleCommand, ErrorOr<RuleConfiguration?>>
    {
        public async Task<ErrorOr<RuleConfiguration?>> Handle(UpdateRuleCommand request, CancellationToken cancellationToken)
        {
            var existingRule = await ruleRepository.GetByNameAsync(request.RuleName, cancellationToken);

            if (existingRule is null)
            {
                return (RuleConfiguration?)null;
            }

            existingRule.Update(request.Description, request.Expression, request.Parameters, request.IsActive);
           
            await ruleRepository.UpdateAsync(existingRule, cancellationToken);

            
            return existingRule;
        }
    }
}
