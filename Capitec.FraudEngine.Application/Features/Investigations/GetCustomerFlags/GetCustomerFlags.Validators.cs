using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Investigations.GetCustomerFlags
{
    public class GetCustomerFlagsValidator : AbstractValidator<GetCustomerFlagsQuery>
    {
        public GetCustomerFlagsValidator() => RuleFor(x => x.CustomerId).NotEmpty();
    }
}
