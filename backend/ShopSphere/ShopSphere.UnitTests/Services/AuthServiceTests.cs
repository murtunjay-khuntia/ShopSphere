using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using ShopSphere.Application.DTOs.Auth;
using ShopSphere.Application.Interfaces.Repositories;
using ShopSphere.Application.Interfaces.Services;
using ShopSphere.Application.Services;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;
using ShopSphere.Domain.Exceptions;
using ShopSphere.Infrastructure.Security;

namespace ShopSphere.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGenMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _tokenGenMock = new Mock<IJwtTokenGenerator>();

        _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        _sut = new AuthService(_uowMock.Object, _tokenGenMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ShouldCreateCustomerAndReturnTokens()
    {
        var req = new RegisterRequestDto("John Doe", "john@test.com", "Pass@123");
        _userRepoMock.Setup(r => r.GetByEmailAsync("john@test.com", default)).ReturnsAsync((User?)null);
        _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns(("jwt_token", DateTime.UtcNow.AddMinutes(15)));
        _tokenGenMock.Setup(t => t.GenerateRefreshToken(It.IsAny<Guid>())).Returns(new RefreshToken { Token = "ref_token" });

        var result = await _sut.RegisterAsync(req);

        result.Email.Should().Be("john@test.com");
        result.Role.Should().Be("Customer");
        result.AccessToken.Should().Be("jwt_token");
        result.RefreshToken.Should().Be("ref_token");
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ShouldThrowBadRequestException()
    {
        var req = new RegisterRequestDto("John", "dup@test.com", "Pass@123");
        _userRepoMock.Setup(r => r.GetByEmailAsync("dup@test.com", default)).ReturnsAsync(new User { Email = "dup@test.com" });

        Func<Task> act = () => _sut.RegisterAsync(req);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokens()
    {
        var user = new User
        {
            FullName = "Admin",
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync("admin@test.com", default)).ReturnsAsync(user);
        _tokenGenMock.Setup(t => t.GenerateAccessToken(user)).Returns(("access_123", DateTime.UtcNow.AddMinutes(15)));
        _tokenGenMock.Setup(t => t.GenerateRefreshToken(user.Id)).Returns(new RefreshToken { Token = "refresh_123" });

        var result = await _sut.LoginAsync(new LoginRequestDto("admin@test.com", "Admin@123"));

        result.Role.Should().Be("Admin");
        result.AccessToken.Should().Be("access_123");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowUnauthorizedException()
    {
        var user = new User { Email = "a@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("RightPass") };
        _userRepoMock.Setup(r => r.GetByEmailAsync("a@test.com", default)).ReturnsAsync(user);

        Func<Task> act = () => _sut.LoginAsync(new LoginRequestDto("a@test.com", "WrongPass"));

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldThrowUnauthorizedException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("missing@test.com", default)).ReturnsAsync((User?)null);

        Func<Task> act = () => _sut.LoginAsync(new LoginRequestDto("missing@test.com", "Pass@123"));

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidActiveToken_ShouldRotateTokens()
    {
        var oldToken = new RefreshToken { Token = "old_rt", ExpiresAt = DateTime.UtcNow.AddDays(5), IsRevoked = false };
        var user = new User { FullName = "User", Email = "u@test.com", RefreshTokens = new List<RefreshToken> { oldToken } };

        _userRepoMock.Setup(r => r.GetByRefreshTokenAsync("old_rt", default)).ReturnsAsync(user);
        _tokenGenMock.Setup(t => t.GenerateAccessToken(user)).Returns(("new_jwt", DateTime.UtcNow.AddMinutes(15)));
        _tokenGenMock.Setup(t => t.GenerateRefreshToken(user.Id)).Returns(new RefreshToken { Token = "new_rt" });

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequestDto("old_rt"));

        oldToken.IsRevoked.Should().BeTrue();
        oldToken.ReplacedByToken.Should().Be("new_rt");
        result.RefreshToken.Should().Be("new_rt");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenAlreadyRevoked_ShouldRevokeAllActiveTokensAndThrow()
    {
        var compromised = new RefreshToken { Token = "stolen_rt", ExpiresAt = DateTime.UtcNow.AddDays(5), IsRevoked = true };
        var active = new RefreshToken { Token = "active_rt", ExpiresAt = DateTime.UtcNow.AddDays(5), IsRevoked = false };
        var user = new User { RefreshTokens = new List<RefreshToken> { compromised, active } };

        _userRepoMock.Setup(r => r.GetByRefreshTokenAsync("stolen_rt", default)).ReturnsAsync(user);

        Func<Task> act = () => _sut.RefreshTokenAsync(new RefreshTokenRequestDto("stolen_rt"));

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*reuse detected*");
        active.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenExpired_ShouldThrowUnauthorizedException()
    {
        var expired = new RefreshToken { Token = "exp_rt", ExpiresAt = DateTime.UtcNow.AddMinutes(-1), IsRevoked = false };
        var user = new User { RefreshTokens = new List<RefreshToken> { expired } };

        _userRepoMock.Setup(r => r.GetByRefreshTokenAsync("exp_rt", default)).ReturnsAsync(user);

        Func<Task> act = () => _sut.RefreshTokenAsync(new RefreshTokenRequestDto("exp_rt"));

        await act.Should().ThrowAsync<UnauthorizedException>().WithMessage("*expired*");
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenNotFound_ShouldThrowUnauthorizedException()
    {
        _userRepoMock.Setup(r => r.GetByRefreshTokenAsync("unknown", default)).ReturnsAsync((User?)null);

        Func<Task> act = () => _sut.RefreshTokenAsync(new RefreshTokenRequestDto("unknown"));

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RevokeTokenAsync_WhenTokenActive_ShouldMarkAsRevoked()
    {
        var token = new RefreshToken { Token = "rt_1", ExpiresAt = DateTime.UtcNow.AddDays(2), IsRevoked = false };
        var user = new User { RefreshTokens = new List<RefreshToken> { token } };
        _userRepoMock.Setup(r => r.GetByRefreshTokenAsync("rt_1", default)).ReturnsAsync(user);

        await _sut.RevokeTokenAsync("rt_1");

        token.IsRevoked.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public void JwtTokenGenerator_ShouldGenerateValidJwtAndRefreshToken()
    {
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "SuperSecretUnitTestKey_AtLeast32CharactersLong!",
            ["JwtSettings:Issuer"] = "TestIssuer",
            ["JwtSettings:Audience"] = "TestAudience",
            ["JwtSettings:AccessTokenExpirationMinutes"] = "15",
            ["JwtSettings:RefreshTokenExpirationDays"] = "7"
        };
        IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(configData).Build();
        var generator = new JwtTokenGenerator(config);
        var user = new User { FullName = "Tester", Email = "t@test.com", Role = UserRole.Admin };

        var (accessToken, expiresAt) = generator.GenerateAccessToken(user);
        var refreshToken = generator.GenerateRefreshToken(user.Id);

        accessToken.Should().NotBeNullOrWhiteSpace();
        expiresAt.Should().BeAfter(DateTime.UtcNow);
        refreshToken.Token.Should().NotBeNullOrWhiteSpace();
        refreshToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_IsActive_ShouldBeFalseWhenExpiredOrRevoked()
    {
        var expiredToken = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddSeconds(-10), IsRevoked = false };
        var revokedToken = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(1), IsRevoked = true };

        expiredToken.IsActive.Should().BeFalse();
        revokedToken.IsActive.Should().BeFalse();
    }
}
