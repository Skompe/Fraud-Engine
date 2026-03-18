using Capitec.FraudEngine.Application.Abstractions.Authentication;
using Capitec.FraudEngine.Application.Features.Identity.Login;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using Moq;

namespace Capitec.FraudEngine.Tests.Application.Features.Identity;

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private readonly LoginQueryHandler _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginQueryHandler(
            _userRepoMock.Object,
            _jwtMock.Object,
            _passwordServiceMock.Object,
            _refreshTokenRepoMock.Object,
            _uowMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResponseWithTokens()
    {
        // Arrange
        var user = new User("analyst01", "Analyst", "hash");
        _userRepoMock
            .Setup(r => r.GetByUsernameAsync("analyst01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordServiceMock
            .Setup(p => p.VerifyPassword(user, "pass"))
            .Returns(true);
        _jwtMock
            .Setup(j => j.GenerateToken(user))
            .Returns("access-token-value");
        _refreshTokenRepoMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new LoginQuery("analyst01", "pass"), CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("access-token-value", result.Value.AccessToken);
        Assert.Equal("Bearer", result.Value.TokenType);
        Assert.Equal(3600, result.Value.ExpiresIn);
        Assert.Equal("analyst01", result.Value.Username);
        Assert.Equal("Analyst", result.Value.Role);
        Assert.NotEmpty(result.Value.RefreshToken);

        _refreshTokenRepoMock.Verify(r => r.AddAsync(It.Is<RefreshToken>(t => t.UserId == user.Id)), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(new LoginQuery("ghost", "pass"), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Unauthorized, result.FirstError.Type);
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var user = new User("inactive", "Analyst", "hash");
        user.Deactivate();

        _userRepoMock
            .Setup(r => r.GetByUsernameAsync("inactive", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new LoginQuery("inactive", "pass"), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Unauthorized, result.FirstError.Type);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var user = new User("analyst01", "Analyst", "hash");
        _userRepoMock
            .Setup(r => r.GetByUsernameAsync("analyst01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordServiceMock
            .Setup(p => p.VerifyPassword(user, "wrong"))
            .Returns(false);

        // Act
        var result = await _handler.Handle(new LoginQuery("analyst01", "wrong"), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Unauthorized, result.FirstError.Type);

        _refreshTokenRepoMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UsernameIsCaseInsensitive_NormalisedBeforeLookup()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _handler.Handle(new LoginQuery("ADMIN", "pass"), CancellationToken.None);

        // Assert
        _userRepoMock.Verify(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()), Times.Once);
    }
}
