using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Lifecycle;
using MSMS.Domain.Constants;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/lifecycle")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class LifecycleController(IStudentLifecycleService lifecycle) : ControllerBase
{
    [HttpPost("promotions")]
    public async Task<IActionResult> Promote([FromBody] PromoteStudentDto dto, CancellationToken cancellationToken)
    {
        await lifecycle.PromoteAsync(dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("graduations")]
    public async Task<IActionResult> Graduate([FromBody] GraduateStudentDto dto, CancellationToken cancellationToken)
    {
        await lifecycle.GraduateAsync(dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("promotions/auto")]
    public async Task<ActionResult<AutoPromoteResultDto>> AutoPromoteAll([FromBody] AutoPromoteRequestDto dto, CancellationToken cancellationToken)
    {
        return Ok(await lifecycle.AutoPromoteAllAsync(dto, cancellationToken));
    }

    [HttpGet("graduated-students")]
    public async Task<ActionResult<IReadOnlyList<GraduatedStudentDto>>> ListGraduated(CancellationToken cancellationToken)
    {
        return Ok(await lifecycle.ListGraduatedAsync(cancellationToken));
    }
}
