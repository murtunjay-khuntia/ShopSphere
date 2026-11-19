using ShopSphere.Application.DTOs.Auth;
using ShopSphere.Application.Interfaces.Repositories;
using ShopSphere.Application.Interfaces.Services;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUnitOfWork unitOfWork, IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new BadRequestException("An account with this email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Customer
        };

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.Users.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            accessToken,
            refreshToken.Token,
            expiresAt);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        await _unitOfWork.Users.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            accessToken,
            refreshToken.Token,
            expiresAt);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var existingToken = user.RefreshTokens.Single(rt => rt.Token == request.RefreshToken);

        // Security: If a revoked token is reused, revoke ALL active tokens for this user
        if (existingToken.IsRevoked)
        {
            foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
            {
                token.IsRevoked = true;
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Refresh token reuse detected. Please log in again.");
        }

        if (existingToken.IsExpired)
        {
            throw new UnauthorizedException("Refresh token has expired. Please log in again.");
        }

        // Rotate refresh token
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);
        existingToken.IsRevoked = true;
        existingToken.ReplacedByToken = newRefreshToken.Token;

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user);

        await _unitOfWork.Users.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            accessToken,
            newRefreshToken.Token,
            expiresAt);
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(refreshToken, cancellationToken);
        if (user is null) return;

        var token = user.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken);
        if (token is not null && token.IsActive)
        {
            token.IsRevoked = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
