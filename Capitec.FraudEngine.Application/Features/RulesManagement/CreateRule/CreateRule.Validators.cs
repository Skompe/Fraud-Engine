using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.CreateRule
{
    public class CreateRuleValidator : AbstractValidator<CreateRuleCommand>
    {
        public CreateRuleValidator()
        {
            RuleFor(x => x.RuleName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(500);

          
            RuleFor(x => x.Expression).NotEmpty()
                .WithMessage("A valid rule expression (e.g., 'tx.Amount > 100') is required.");
        }
    }
}
