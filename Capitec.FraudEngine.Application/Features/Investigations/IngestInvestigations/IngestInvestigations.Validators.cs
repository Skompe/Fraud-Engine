using FluentValidation;
using Capitec.FraudEngine.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Investigations.IngestInvestigations
{
    public class IngestInvestigationsCommandValidator : AbstractValidator<IngestInvestigationsCommand>
    {
        public IngestInvestigationsCommandValidator()
        {
            
            RuleFor(x => x.Investigations)
                .NotEmpty().WithMessage("You must provide at least one investigation to ingest.")
                .Must(list => list.Count <= 1000).WithMessage("Cannot ingest more than 1000 items per batch.");

            
            RuleForEach(x => x.Investigations).ChildRules(item =>
            {
                item.RuleFor(i => i.TransactionId).NotEmpty();
                item.RuleFor(i => i.CustomerId).NotEmpty();
                item.RuleFor(i => i.Source).NotEmpty().MaximumLength(100);
                item.RuleFor(i => i.Reason).NotEmpty().MaximumLength(1000);
                item.RuleFor(i => i.Severity)
                    .Must(s => DomainConstants.Severity.All.Contains((s ?? string.Empty).ToUpperInvariant()))
                    .WithMessage("Severity must be Low, Medium, High, or Critical.");
            });
        }
    }
}
