using Capitec.FraudEngine.Application.Abstractions.Authentication;
using Capitec.FraudEngine.Application.Constants;
using Capitec.FraudEngine.Application.Features.Identity.RefreshToken;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using Moq;

namespace Capitec.FraudEngine.Tests.Application.Features.Identity;

public class RefreshTokenHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepoMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private readonly FraudEngine.Application.Features.Identity.RefreshToken.RefreshTokenHandler _handler;

    public RefreshTokenHandlerTests()
    {
        _handler = new FraudEngine.Application.Features.Identity.RefreshToken.RefreshTokenHandler(
            _refreshTokenRepoMock.Object,
            _userRepoMock.Object,
            _auditLogRepoMock.Object,
            _jwtMock.Object,
            _uowMock.Object);
    }

    private static FraudEngine.Domain.Entities.RefreshToken ValidToken(Guid userId) => new()
    {
        UserId = userId,
        Token = "old-token",
        IssuedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(7),
        RotationCount = 0
    };

    [Fact]
    public async Task Handle_ValidToken_RotatesTokenAndReturnsNewAccessToken()
    {
        // Arrange
        var user = new User("analyst", "Analyst", "hash");
        var stored = ValidToken(user.Id);

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync("old-token"))
            .ReturnsAsync(stored);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtMock
            .Setup(j => j.GenerateToken(user))
            .Returns("new-access-token");

        // Act
        var result = await _handler.Handle(
            new RefreshTokenCommand("old-token"),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("new-access-token", result.Value.AccessToken);
        Assert.Equal(IdentityConstants.Tokens.Bearer, result.Value.TokenType);
        Assert.NotEmpty(result.Value.NewRefreshToken);

        _refreshTokenRepoMock.Verify(r => r.UpdateAsync(stored), Times.Once);
        _refreshTokenRepoMock.Verify(r => r.AddAsync(It.IsAny<FraudEngine.Domain.Entities.RefreshToken>()), Times.Once);
        _auditLogRepoMock.Verify(r => r.AddAsync(It.Is<AuditLog>(
            a => a.Action == AuditLog.Actions.TokenRefreshed)), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsUnauthorized()
    {
        // Arrange
        _refreshTokenRepoMock
            .Setup<Task<FraudEngine.Domain.Entities.RefreshToken>>(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync<IRefreshTokenRepository, FraudEngine.Domain.Entities.RefreshToken>((FraudEngine.Domain.Entities.RefreshToken?)null);

        // Act
        var result = await _handler.Handle(
            new RefreshTokenCommand("no-such-token"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Unauthorized, result.FirstError.Type);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var expired = new FraudEngine.Domain.Entities.RefreshToken
        {
            UserId = Guid.NewGuid(),
            Token = "expired-token",
            IssuedAt = DateTime.UtcNow.AddDays(-14),
            ExpiresAt = DateTime.UtcNow.AddDays(-7) // already expired
        };

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync("expired-token"))
            .ReturnsAsync(expired);

        // Act
        var result = await _handler.Handle(
            new RefreshTokenCommand("expired-token"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Unauthorized, result.FirstError.Type);
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var user = new User("gone", "Analyst", "hash");
        user.Deactivate();
        var stored = ValidToken(user.Id);

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync("old-token"))
            .ReturnsAsync(stored);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(
            new RefreshTokenCommand("old-token"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Unauthorized, result.FirstError.Type);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
