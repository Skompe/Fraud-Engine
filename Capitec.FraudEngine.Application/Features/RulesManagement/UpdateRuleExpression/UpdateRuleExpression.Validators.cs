using Capitec.FraudEngine.Domain.Abstractions.Data;
using ErrorOr;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.UpdateRuleExpression
{
    public class UpdateRuleExpressionValidator : AbstractValidator<UpdateRuleExpressionCommand>
    {
        public UpdateRuleExpressionValidator()
        {
            RuleFor(x => x.RuleName).NotEmpty();
            RuleFor(x => x.NewExpression).NotEmpty();
        }
    }


}
