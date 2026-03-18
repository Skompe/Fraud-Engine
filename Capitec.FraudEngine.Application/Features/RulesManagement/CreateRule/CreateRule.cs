using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.CreateRule
{
    public record CreateRuleCommand(string RuleName, string Description, string Expression) : IRequest<ErrorOr<Success>>;

    internal class CreateRuleHandler(IRuleRepository repository,IUnitOfWork unitOfWork): IRequestHandler<CreateRuleCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(CreateRuleCommand request, CancellationToken ct)
        {
            
            if (await repository.ExistsAsync(request.RuleName, ct))
            {
                return Error.Conflict("Rule.Duplicate", $"A rule named '{request.RuleName}' already exists.");
            }

            var rule = new RuleConfiguration(request.RuleName, request.Description, request.Expression);

            await repository.AddAsync(rule, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success;
        }
    }
}
