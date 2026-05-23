using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Exams;
using MSMS.Application.DTOs.Student;
using MSMS.Domain.Constants;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/student")]
[Authorize(Roles = RoleNames.Student)]
public sealed class StudentPortalController(IStudentPortalService studentPortal) : ControllerBase
{
    [HttpGet("me/portal")]
    public async Task<ActionResult<StudentPortalDto>> MyPortal(CancellationToken cancellationToken)
    {
        return Ok(await studentPortal.GetMyPortalAsync(cancellationToken));
    }

    [HttpGet("me/exam-results")]
    public async Task<ActionResult<IReadOnlyList<ExamResultDto>>> MyExamResults(CancellationToken cancellationToken)
    {
        return Ok(await studentPortal.GetMyExamResultsAsync(cancellationToken));
    }
}
