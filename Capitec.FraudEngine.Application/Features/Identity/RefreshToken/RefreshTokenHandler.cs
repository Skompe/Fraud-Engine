using Capitec.FraudEngine.Application.Abstractions.Authentication;
using Capitec.FraudEngine.Application.Constants;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Capitec.FraudEngine.Application.Features.Identity.RefreshToken
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<RefreshTokenResponse>>;
    public record RefreshTokenResponse(string AccessToken,string TokenType,int ExpiresIn,string NewRefreshToken,bool ShouldRotateImmediately);

    public class RefreshTokenHandler(IRefreshTokenRepository refreshTokenRepository,IUserRepository userRepository,IAuditLogRepository auditLogRepository
        ,IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork) : IRequestHandler<RefreshTokenCommand, ErrorOr<RefreshTokenResponse>>
    {
        public async Task<ErrorOr<RefreshTokenResponse>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var storedToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (storedToken == null || !storedToken.IsValid)
            {
                return Error.Unauthorized(
                    code: ErrorCodes.Identity.InvalidRefreshTokenCode,
                    description: "Refresh token is invalid or expired");
            }

            var user = await userRepository.GetByIdAsync(storedToken.UserId);
            if (user == null || !user.IsActive)
            {
                return Error.Unauthorized(
                    code: ErrorCodes.Identity.UserNotFoundCode,
                    description: "User not found or inactive");
            }

            var newAccessToken = jwtTokenGenerator.GenerateToken(user);

            var newRefreshTokenValue = Guid.NewGuid().ToString("N");
            var newRefreshToken = new Domain.Entities.RefreshToken
            {
                UserId = storedToken.UserId,
                Token = newRefreshTokenValue,
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7), 
                RotationCount = storedToken.RotationCount + 1
            };

            storedToken.Revoke(newRefreshTokenValue);
            await refreshTokenRepository.UpdateAsync(storedToken);
            await refreshTokenRepository.AddAsync(newRefreshToken);

            var auditLog = AuditLog.Create(
                fraudFlagId: Guid.Empty,
                userId: user.Id,
                action: AuditLog.Actions.TokenRefreshed,
                description: "User refreshed access token.",
                sourceSystem: IdentityConstants.SourceSystems.FraudEngineApi);

            await auditLogRepository.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new RefreshTokenResponse(
                AccessToken: newAccessToken,
                TokenType: IdentityConstants.Tokens.Bearer,
                ExpiresIn: IdentityConstants.Tokens.AccessTokenExpiresInSeconds,
                NewRefreshToken: newRefreshTokenValue,
                ShouldRotateImmediately: storedToken.RotationCount > 10); 
        }
    }
}
