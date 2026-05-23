using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Parent;
using MSMS.Domain.Constants;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/parent")]
[Authorize(Roles = RoleNames.Parent)]
public sealed class ParentPortalController(IParentPortalService parentPortal) : ControllerBase
{
    [HttpGet("children")]
    public async Task<ActionResult<IReadOnlyList<LinkedStudentDto>>> Children(CancellationToken cancellationToken)
    {
        return Ok(await parentPortal.GetLinkedStudentsAsync(cancellationToken));
    }
}
