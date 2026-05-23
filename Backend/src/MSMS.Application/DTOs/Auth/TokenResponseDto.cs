namespace MSMS.Application.DTOs.Auth;

public sealed record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc,
    IReadOnlyList<string> Roles);
