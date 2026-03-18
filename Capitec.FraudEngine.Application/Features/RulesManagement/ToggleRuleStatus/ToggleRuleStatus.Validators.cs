using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.ToggleRuleStatus
{
    public class ToggleRuleStatusValidator : AbstractValidator<ToggleRuleStatusCommand>
    {
        public ToggleRuleStatusValidator() => RuleFor(x => x.RuleName).NotEmpty();
    }
}
