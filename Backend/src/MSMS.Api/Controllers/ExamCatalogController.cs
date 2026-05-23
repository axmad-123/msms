using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Exams;
using MSMS.Domain.Constants;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/exam-catalog")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class ExamCatalogController(IExamCatalogService exams) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExamDto>>> List([FromQuery] string? academicYear, CancellationToken cancellationToken)
    {
        return Ok(await exams.ListAsync(academicYear, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateExamDto dto, CancellationToken cancellationToken)
    {
        var id = await exams.CreateAsync(dto, cancellationToken);
        return Created($"/api/exam-catalog/{id}", id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateExamDto dto, CancellationToken cancellationToken)
    {
        await exams.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await exams.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
