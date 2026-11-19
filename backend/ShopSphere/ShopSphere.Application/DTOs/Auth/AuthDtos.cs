using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Application.DTOs.Auth;

public record RegisterRequestDto(
    [Required, MaxLength(150)] string FullName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(6)] string Password
);

public record LoginRequestDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record RefreshTokenRequestDto(
    [Required] string RefreshToken
);

public record AuthResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt
);
