using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Abstractions.Rules;
using Capitec.FraudEngine.Application.Constants;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.UpdateRuleThreshold
{
    
    public record UpdateRuleThresholdCommand(string RuleName, string NewParameters) : IRequest<ErrorOr<Success>>;
    public class UpdateRuleThresholdHandler(IRuleRepository ruleRepository,IUnitOfWork unitOfWork,IFraudRuleManager ruleManager) : IRequestHandler<UpdateRuleThresholdCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(UpdateRuleThresholdCommand request, CancellationToken ct)
        {
            var rule = await ruleRepository.GetByNameAsync(request.RuleName, ct);
            if (rule is null) return Error.NotFound(ErrorCodes.Rules.NotFoundCode, ErrorCodes.Rules.DoesNotExistMessage);

            
            try
            {
                using var jsonDoc = JsonDocument.Parse(request.NewParameters);
            }
            catch (JsonException)
            {
                return Error.Validation(ErrorCodes.Rules.InvalidParametersCode, ErrorCodes.Rules.InvalidParametersMessage);
            }

            rule.UpdateParameters(request.NewParameters);
            ruleRepository.Update(rule);

            await unitOfWork.SaveChangesAsync(ct);

            
            await ruleManager.SynchronizeAllRulesAsync(ct);

            return Result.Success;
        }
    }
}
