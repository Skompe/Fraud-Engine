using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Transactions.ProcessSyncTransaction
{
    public class ProcessSyncTransactionValidator : AbstractValidator<ProcessSyncTransactionCommand>
    {
        public ProcessSyncTransactionValidator()
        {
            RuleFor(x => x.TransactionId).NotEmpty();
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().Length(3);
        }
    }
}
