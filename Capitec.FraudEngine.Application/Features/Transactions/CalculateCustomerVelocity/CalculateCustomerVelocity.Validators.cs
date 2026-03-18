using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Transactions.CalculateCustomerVelocity
{
    public class GetCustomerVelocityValidator : AbstractValidator<GetCustomerVelocityQuery>
    {
        public GetCustomerVelocityValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.WindowInMinutes).GreaterThan(0);
        }
    }
}
