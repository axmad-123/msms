using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Exams;
using MSMS.Domain.Constants;
using MSMS.Domain.Enums;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/exam-results")]
[Authorize]
public sealed class ExamResultsController(IExamResultService examResults) : ControllerBase
{
    [HttpGet("students/{studentId:guid}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Parent},{RoleNames.Student},{RoleNames.Teacher}")]
    public async Task<ActionResult<IReadOnlyList<ExamResultDto>>> ForStudent(Guid studentId, CancellationToken cancellationToken)
    {
        return Ok(await examResults.GetForStudentAsync(studentId, cancellationToken));
    }

    [HttpGet("classes/{classId:guid}/subjects/{subjectId:guid}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Teacher}")]
    public async Task<ActionResult<IReadOnlyList<ExamResultDto>>> ForClassSubject(
        Guid classId,
        Guid subjectId,
        [FromQuery] int? examType,
        CancellationToken cancellationToken)
    {
        return Ok(await examResults.GetForClassSubjectAsync(classId, subjectId, examType, cancellationToken));
    }

    [HttpGet("classes/{classId:guid}/subjects/{subjectId:guid}/export")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> ExportCsv(
        Guid classId,
        Guid subjectId,
        [FromQuery] int? examType,
        CancellationToken cancellationToken)
    {
        var rows = await examResults.GetForClassSubjectAsync(classId, subjectId, examType, cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Student,Subject,ExamType,Marks,MaxMarks,Grade,AcademicYear");
        foreach (var r in rows)
        {
            var type = Enum.IsDefined(typeof(ExamType), r.ExamType) ? ((ExamType)r.ExamType).ToString() : r.ExamType.ToString();
            sb.AppendLine($"\"{r.StudentName}\",\"{r.SubjectName}\",{type},{r.Marks},{r.MaxMarks},{r.Grade},{r.AcademicYear}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"exam-results-{classId:N}.csv");
    }

    [HttpPost("bulk")]
    [Authorize(Roles = RoleNames.Teacher)]
    public async Task<IActionResult> BulkUpsert([FromBody] UpsertExamResultsDto dto, CancellationToken cancellationToken)
    {
        await examResults.UpsertResultsAsync(dto, cancellationToken);
        return NoContent();
    }
}
