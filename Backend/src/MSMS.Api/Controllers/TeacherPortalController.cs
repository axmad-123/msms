using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.People;
using MSMS.Application.DTOs.School;
using MSMS.Domain.Constants;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = RoleNames.Teacher)]
public sealed class TeacherPortalController(ITeacherPortalService teacherPortal) : ControllerBase
{
    [HttpGet("me/assignments")]
    public async Task<ActionResult<IReadOnlyList<TeacherSubjectDto>>> MyAssignments(CancellationToken cancellationToken)
    {
        return Ok(await teacherPortal.GetMyAssignmentsAsync(cancellationToken));
    }

    [HttpGet("classes/{classId:guid}/students")]
    public async Task<ActionResult<IReadOnlyList<StudentListItemDto>>> ClassStudents(Guid classId, CancellationToken cancellationToken)
    {
        return Ok(await teacherPortal.GetClassStudentsAsync(classId, cancellationToken));
    }
}
