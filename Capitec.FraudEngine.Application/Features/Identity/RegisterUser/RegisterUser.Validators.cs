using FluentValidation;
using Capitec.FraudEngine.Application.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Identity.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .EmailAddress().WithMessage("Username must be a valid Capitec email address.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.Role)
                .Must(r => r is IdentityConstants.Roles.Admin or IdentityConstants.Roles.Analyst or IdentityConstants.Roles.System)
                .WithMessage($"Role must be {IdentityConstants.Roles.Admin}, {IdentityConstants.Roles.Analyst}, or {IdentityConstants.Roles.System}.");
        }
    }
}
