using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Capitec.FraudEngine.Application.Features.RulesManagement.UpdateRuleThreshold
{
    public class UpdateRuleThresholdValidator : AbstractValidator<UpdateRuleThresholdCommand>
    {
        public UpdateRuleThresholdValidator()
        {
            RuleFor(x => x.RuleName).NotEmpty();
            RuleFor(x => x.NewParameters).NotEmpty()
             .Must(BeValidJson);
            
        }

        private bool BeValidJson(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
