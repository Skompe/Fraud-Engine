using FluentValidation;
using Capitec.FraudEngine.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Investigations.ResolveFraudFlag
{
    public class ResolveFraudFlagValidator : AbstractValidator<ResolveFraudFlagCommand>
    {
        public ResolveFraudFlagValidator()
        {
            RuleFor(x => x.FlagId).NotEmpty();
            RuleFor(x => x.ResolutionStatus).Must(s => s is DomainConstants.FraudStatus.FalsePositive or DomainConstants.FraudStatus.ConfirmedFraud)
                .WithMessage($"Status must be {DomainConstants.FraudStatus.FalsePositive} or {DomainConstants.FraudStatus.ConfirmedFraud}");
            RuleFor(x => x.AnalystNotes).NotEmpty();
        }
    }
}
