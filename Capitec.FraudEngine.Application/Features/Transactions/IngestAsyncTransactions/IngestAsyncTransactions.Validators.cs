using Capitec.FraudEngine.Application.Features.Transactions.IngestAsyncTransaction;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Transactions.IngestAsyncTransactions
{
    public class IngestAsyncTransactionsCommandValidator : AbstractValidator<IngestAsyncTransactionsCommand>
    {
        public IngestAsyncTransactionsCommandValidator()
        {
           
            RuleFor(x => x.Transactions)
                .NotEmpty().WithMessage("You must provide at least one transaction to ingest.")
                .Must(list => list.Count <= 2000).WithMessage("Cannot ingest more than 2000 transactions per batch.");

           
            RuleForEach(x => x.Transactions).ChildRules(item =>
            {
                item.RuleFor(i => i.TransactionId).NotEmpty().WithMessage("TransactionId is required.");
                item.RuleFor(i => i.CustomerId).NotEmpty().WithMessage("CustomerId is required.");

                item.RuleFor(i => i.Amount)
                    .GreaterThan(0).WithMessage("Transaction amount must be strictly greater than zero.");

                item.RuleFor(i => i.Currency)
                    .NotEmpty()
                    .Length(3).WithMessage("Currency must be a 3-letter ISO code (e.g., ZAR).");

                item.RuleFor(i => i.Timestamp)
                    .NotEmpty()
                    .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5)) 
                    .WithMessage("Transaction timestamp cannot be in the future.");
            });
        }
    }
}
