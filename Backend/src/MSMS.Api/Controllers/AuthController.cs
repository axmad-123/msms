using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Auth;
using MSMS.Domain.Constants;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await auth.LoginAsync(request, cancellationToken));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
    {
        return Ok(await auth.RefreshAsync(request, cancellationToken));
    }

    [HttpPost("register")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request, CancellationToken cancellationToken)
    {
        await auth.RegisterUserAsync(request, cancellationToken);
        return NoContent();
    }
}
