using Capitec.FraudEngine.Application.Abstractions.Authentication;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Identity.Login
{

    public record LoginQuery(string Username, string Password) : IRequest<ErrorOr<LoginResponse>>;
    public record LoginResponse(string Token, string Username, string Role);
    public class LoginQueryHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IPasswordService passwordService): IRequestHandler<LoginQuery, ErrorOr<LoginResponse>>
    {
        public async Task<ErrorOr<LoginResponse>> Handle(LoginQuery request, CancellationToken ct)
        {
            var normalizedUsername = request.Username.Trim().ToLowerInvariant();

            var user = await userRepository.GetByUsernameAsync(normalizedUsername, ct);

           
            if (user is null || !user.IsActive)
            {
                return Error.Unauthorized(description: "Invalid credentials.");
            }

            if (!passwordService.VerifyPassword(user, request.Password))
            {
                return Error.Unauthorized(description: "Invalid credentials.");
            }

            var token = jwtTokenGenerator.GenerateToken(user);

            return new LoginResponse(token, user.Username, user.Role);
        }
    }
}
