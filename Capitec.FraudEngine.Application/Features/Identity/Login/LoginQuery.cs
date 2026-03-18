using Capitec.FraudEngine.Application.Abstractions.Authentication;
using Capitec.FraudEngine.Application.Constants;
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
    public record LoginResponse(string AccessToken, string RefreshToken, string TokenType, int ExpiresIn, string Username, string Role);
    public class LoginQueryHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordService passwordService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork): IRequestHandler<LoginQuery, ErrorOr<LoginResponse>>
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
            var refreshTokenValue = Guid.NewGuid().ToString("N");

            var refreshToken = new Domain.Entities.RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenValue,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await refreshTokenRepository.AddAsync(refreshToken);
            await unitOfWork.SaveChangesAsync(ct);

            return new LoginResponse(
                token,
                refreshTokenValue,
                IdentityConstants.Tokens.Bearer,
                IdentityConstants.Tokens.AccessTokenExpiresInSeconds,
                user.Username,
                user.Role);
        }
    }
}
