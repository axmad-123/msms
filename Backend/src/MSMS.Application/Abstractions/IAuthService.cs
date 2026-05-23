using MSMS.Application.DTOs.Auth;

namespace MSMS.Application.Abstractions;

public interface IAuthService
{
    Task<TokenResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default);
    Task RegisterUserAsync(RegisterUserRequestDto request, CancellationToken cancellationToken = default);
}
