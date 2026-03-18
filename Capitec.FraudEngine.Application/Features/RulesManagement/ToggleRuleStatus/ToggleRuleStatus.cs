
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Application.Constants;
using ErrorOr;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.ToggleRuleStatus
{
    public record ToggleRuleStatusCommand(string RuleName, bool IsActive) : IRequest<ErrorOr<Success>>;

    public class ToggleRuleStatusHandler(IRuleRepository repository, IUnitOfWork unitOfWork): IRequestHandler<ToggleRuleStatusCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(ToggleRuleStatusCommand request, CancellationToken ct)
        {
            var rule = await repository.GetByNameAsync(request.RuleName, ct);
            if (rule is null) return Error.NotFound(ErrorCodes.Rules.NotFoundCode, string.Format(ErrorCodes.Rules.NotFoundMessageTemplate, request.RuleName));

            rule.Toggle(request.IsActive);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
