using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Common;
using MSMS.Application.DTOs.People;
using MSMS.Domain.Constants;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/admin/people")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AdminPeopleController(IPeopleAdminService people, IMediaStorageService media) : ControllerBase
{
    [HttpGet("students")]
    public async Task<ActionResult<PagedResult<StudentListItemDto>>> ListStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(await people.ListStudentsAsync(page, pageSize, cancellationToken));
    }

    [HttpGet("students/fee-preview")]
    public async Task<ActionResult<decimal>> FeePreview([FromQuery] Guid? classId, CancellationToken cancellationToken)
    {
        return Ok(await people.GetSuggestedMonthlyFeeAsync(classId, cancellationToken));
    }

    [HttpGet("students/{id:guid}")]
    public async Task<ActionResult<StudentDetailsDto>> GetStudent(Guid id, CancellationToken cancellationToken)
    {
        var student = await people.GetStudentAsync(id, cancellationToken);
        return student is null ? NotFound() : Ok(student);
    }

    [HttpPost("students/upload-photo")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<object>> UploadPhoto(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded." });
        }

        await using var stream = file.OpenReadStream();
        var url = await media.UploadStudentPhotoAsync(stream, file.FileName, cancellationToken);
        return Ok(new { url });
    }

    [HttpPost("students")]
    public async Task<ActionResult<CreateStudentResultDto>> CreateStudent([FromBody] CreateStudentDto dto, CancellationToken cancellationToken)
    {
        var result = await people.CreateStudentAsync(dto, cancellationToken);
        return Created($"/api/admin/people/students/{result.StudentId}", result);
    }

    [HttpPut("students/{id:guid}")]
    public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentDto dto, CancellationToken cancellationToken)
    {
        await people.UpdateStudentAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("students/{id:guid}")]
    public async Task<IActionResult> DeleteStudent(Guid id, CancellationToken cancellationToken)
    {
        await people.DeleteStudentAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("parents")]
    public async Task<ActionResult<IReadOnlyList<ParentListItemDto>>> ListParents(CancellationToken cancellationToken)
    {
        return Ok(await people.ListParentsAsync(cancellationToken));
    }

    [HttpGet("parents/{id:guid}")]
    public async Task<ActionResult<ParentDetailsDto>> GetParent(Guid id, CancellationToken cancellationToken)
    {
        var parent = await people.GetParentAsync(id, cancellationToken);
        return parent is null ? NotFound() : Ok(parent);
    }

    [HttpPost("parents")]
    public async Task<ActionResult<Guid>> CreateParent([FromBody] CreateParentDto dto, CancellationToken cancellationToken)
    {
        var id = await people.CreateParentAsync(dto, cancellationToken);
        return Created($"/api/admin/people/parents/{id}", id);
    }

    [HttpPut("parents/{id:guid}")]
    public async Task<IActionResult> UpdateParent(Guid id, [FromBody] UpdateParentDto dto, CancellationToken cancellationToken)
    {
        await people.UpdateParentAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("parents/{id:guid}")]
    public async Task<IActionResult> DeleteParent(Guid id, CancellationToken cancellationToken)
    {
        await people.DeleteParentAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("parents/{parentId:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> LinkParentChild(Guid parentId, Guid studentId, CancellationToken cancellationToken)
    {
        await people.LinkParentChildAsync(parentId, studentId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("parents/{parentId:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> UnlinkParentChild(Guid parentId, Guid studentId, CancellationToken cancellationToken)
    {
        await people.UnlinkParentChildAsync(parentId, studentId, cancellationToken);
        return NoContent();
    }

    [HttpGet("teachers")]
    public async Task<ActionResult<IReadOnlyList<TeacherListItemDto>>> ListTeachers(CancellationToken cancellationToken)
    {
        return Ok(await people.ListTeachersAsync(cancellationToken));
    }

    [HttpGet("teachers/{id:guid}")]
    public async Task<ActionResult<TeacherDetailsDto>> GetTeacher(Guid id, CancellationToken cancellationToken)
    {
        var teacher = await people.GetTeacherAsync(id, cancellationToken);
        return teacher is null ? NotFound() : Ok(teacher);
    }

    [HttpPost("teachers")]
    public async Task<ActionResult<Guid>> CreateTeacher([FromBody] CreateTeacherDto dto, CancellationToken cancellationToken)
    {
        var id = await people.CreateTeacherAsync(dto, cancellationToken);
        return Created($"/api/admin/people/teachers/{id}", id);
    }

    [HttpPut("teachers/{id:guid}")]
    public async Task<IActionResult> UpdateTeacher(Guid id, [FromBody] UpdateTeacherDto dto, CancellationToken cancellationToken)
    {
        await people.UpdateTeacherAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("teachers/{id:guid}")]
    public async Task<IActionResult> DeleteTeacher(Guid id, CancellationToken cancellationToken)
    {
        await people.DeleteTeacherAsync(id, cancellationToken);
        return NoContent();
    }
}
