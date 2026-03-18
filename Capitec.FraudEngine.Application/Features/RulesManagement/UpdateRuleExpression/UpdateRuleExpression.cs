using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Application.Constants;
using Capitec.FraudEngine.Application.Abstractions.Caching;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.UpdateRuleExpression
{
    public record UpdateRuleExpressionCommand(string RuleName, string NewExpression) : IRequest<ErrorOr<Success>>;

    public class UpdateRuleExpressionHandler(IRuleRepository repository, IUnitOfWork unitOfWork, IRuleCacheInvalidator cacheInvalidator) : IRequestHandler<UpdateRuleExpressionCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(UpdateRuleExpressionCommand request, CancellationToken ct)
        {
            var rule = await repository.GetByNameAsync(request.RuleName, ct);

            if (rule is null)
            {
                return Error.NotFound(ErrorCodes.Rules.NotFoundCode, string.Format(ErrorCodes.Rules.NotFoundMessageTemplate, request.RuleName));
            }


            rule.UpdateExpression(request.NewExpression);

            await unitOfWork.SaveChangesAsync(ct);

            cacheInvalidator.Invalidate();

            return Result.Success;
        }
    }
}
