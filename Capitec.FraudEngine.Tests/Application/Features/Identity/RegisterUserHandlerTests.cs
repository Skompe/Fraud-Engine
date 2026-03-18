using Capitec.FraudEngine.Application.Abstractions.Authentication;
using Capitec.FraudEngine.Application.Features.Identity.RegisterUser;
using Capitec.FraudEngine.Domain.Abstractions.Data;
using Capitec.FraudEngine.Domain.Entities;
using ErrorOr;
using Moq;

namespace Capitec.FraudEngine.Tests.Application.Features.Identity;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();

    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(
            _userRepoMock.Object,
            _uowMock.Object,
            _passwordServiceMock.Object);
    }

    [Fact]
    public async Task Handle_NewUser_CreatesAndReturnsUser()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.ExistsAsync("newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordServiceMock
            .Setup(p => p.HashPassword("pass123"))
            .Returns("hashed-pass");

        User? capturedUser = null;
        _userRepoMock
            .Setup(r => r.Add(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u);

        // Act
        var result = await _handler.Handle(
            new RegisterUserCommand("newuser", "pass123", "Analyst"),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.NotNull(capturedUser);
        Assert.Equal("newuser", capturedUser!.Username);
        Assert.Equal("Analyst", capturedUser.Role);
        Assert.Equal("hashed-pass", capturedUser.PasswordHash);
        Assert.True(capturedUser.IsActive);

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ReturnsConflictError()
    {
        // Arrange
        _userRepoMock
            .Setup(r => r.ExistsAsync("existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(
            new RegisterUserCommand("existing", "pass", "Analyst"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
        Assert.Equal("User.Duplicate", result.FirstError.Code);

        _userRepoMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
