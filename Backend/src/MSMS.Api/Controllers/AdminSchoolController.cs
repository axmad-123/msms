using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.School;
using MSMS.Domain.Constants;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/admin/school")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminSchoolController(ISchoolStructureService school) : ControllerBase
{
    [HttpGet("classes")]
    public async Task<ActionResult<IReadOnlyList<ClassDto>>> ListClasses(CancellationToken cancellationToken)
    {
        return Ok(await school.ListClassesAsync(cancellationToken));
    }

    [HttpGet("classes/{id:guid}")]
    public async Task<ActionResult<ClassDetailsDto>> GetClass(Guid id, CancellationToken cancellationToken)
    {
        var details = await school.GetClassAsync(id, cancellationToken);
        return details is null ? NotFound() : Ok(details);
    }

    [HttpPost("classes")]
    public async Task<ActionResult<Guid>> CreateClass([FromBody] CreateClassDto dto, CancellationToken cancellationToken)
    {
        var id = await school.CreateClassAsync(dto, cancellationToken);
        return Created($"/api/admin/school/classes/{id}", id);
    }

    [HttpPut("classes/{id:guid}")]
    public async Task<IActionResult> UpdateClass(Guid id, [FromBody] CreateClassDto dto, CancellationToken cancellationToken)
    {
        await school.UpdateClassAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("classes/{id:guid}")]
    public async Task<IActionResult> DeleteClass(Guid id, CancellationToken cancellationToken)
    {
        await school.DeleteClassAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("subjects")]
    public async Task<ActionResult<IReadOnlyList<SubjectDto>>> ListSubjects(CancellationToken cancellationToken)
    {
        return Ok(await school.ListSubjectsAsync(cancellationToken));
    }

    [HttpPost("subjects")]
    public async Task<ActionResult<Guid>> CreateSubject([FromBody] CreateSubjectDto dto, CancellationToken cancellationToken)
    {
        var id = await school.CreateSubjectAsync(dto, cancellationToken);
        return Created($"/api/admin/school/subjects/{id}", id);
    }

    [HttpPut("subjects/{id:guid}")]
    public async Task<IActionResult> UpdateSubject(Guid id, [FromBody] CreateSubjectDto dto, CancellationToken cancellationToken)
    {
        await school.UpdateSubjectAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("subjects/{id:guid}")]
    public async Task<IActionResult> DeleteSubject(Guid id, CancellationToken cancellationToken)
    {
        await school.DeleteSubjectAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("teacher-assignments")]
    public async Task<IActionResult> AssignTeacher([FromBody] TeacherSubjectAssignmentDto dto, CancellationToken cancellationToken)
    {
        await school.AssignTeacherAsync(dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("teacher-assignments/{teacherId:guid}/{subjectId:guid}/{classId:guid}")]
    public async Task<IActionResult> UnassignTeacher(Guid teacherId, Guid subjectId, Guid classId, CancellationToken cancellationToken)
    {
        await school.UnassignTeacherAsync(teacherId, subjectId, classId, cancellationToken);
        return NoContent();
    }

    [HttpGet("teacher-assignments")]
    public async Task<ActionResult<IReadOnlyList<TeacherSubjectDto>>> ListAssignments([FromQuery] Guid? teacherId, CancellationToken cancellationToken)
    {
        return Ok(await school.ListAssignmentsAsync(teacherId, cancellationToken));
    }
}
