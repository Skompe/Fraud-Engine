using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.UpdateRule
{
    public class UpdateRuleCommandValidator : AbstractValidator<UpdateRuleCommand>
    {
        public UpdateRuleCommandValidator()
        {
            RuleFor(x => x.RuleName)
                .NotEmpty().WithMessage("Rule Name is required.")
                .MaximumLength(100).WithMessage("Rule Name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description is too long.");

            // Require Expression if Parameters is empty
            RuleFor(x => x.Expression)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.Parameters))
                .WithMessage("An evaluation expression is required if no parameters are provided.")
                .MinimumLength(5)
                .When(x => !string.IsNullOrWhiteSpace(x.Expression))
                .WithMessage("Expression is too short to be valid.");

            // Require Parameters if Expression is empty
            RuleFor(x => x.Parameters)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.Expression))
                .WithMessage("Parameters are required if no evaluation expression is provided.");
        }
    }
}
