using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using Capitec.FraudEngine.Application.Abstractions.Authentication;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Identity.RegisterUser
{
    public record RegisterUserCommand(string Username, string Password, string Role) : IRequest<ErrorOr<User>>;

    public class RegisterUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordService passwordService): IRequestHandler<RegisterUserCommand, ErrorOr<User>>
    {
        public async Task<ErrorOr<User>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            
            bool userExists = await userRepository.ExistsAsync(request.Username, cancellationToken);
            if (userExists)
            {
                return Error.Conflict("User.Duplicate", "This username is already taken.");
            }

            var hashedPassword = passwordService.HashPassword(request.Password);

            var newUser = new User(
                request.Username,
                request.Role,
                hashedPassword
            );

            userRepository.Add(newUser);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return newUser;
        }
    }
}
