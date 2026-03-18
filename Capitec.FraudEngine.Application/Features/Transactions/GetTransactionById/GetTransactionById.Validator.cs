using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Transactions.GetTransactionById
{
    public class GetTransactionByIdValidator : AbstractValidator<GetTransactionByIdQuery>
    {
        public GetTransactionByIdValidator() => RuleFor(x => x.TransactionId).NotEmpty();
    }
}
