using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Attendance;
using MSMS.Domain.Constants;

namespace MSMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AttendanceController(IAttendanceService attendance) : ControllerBase
{
    [HttpPost("sessions")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<Guid>> CreateSession([FromBody] CreateAttendanceSessionDto dto, CancellationToken cancellationToken)
    {
        var id = await attendance.CreateSessionAsync(dto, cancellationToken);
        return Created($"/api/attendance/sessions/{id}", id);
    }

    [HttpPost("sessions/{sessionId:guid}/close")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> CloseSession(Guid sessionId, CancellationToken cancellationToken)
    {
        await attendance.CloseSessionAsync(sessionId, cancellationToken);
        return NoContent();
    }

    [HttpPost("sessions/{sessionId:guid}/records")]
    [Authorize(Roles = RoleNames.Teacher)]
    public async Task<IActionResult> UpsertRecords(Guid sessionId, [FromBody] IReadOnlyList<AttendanceRecordWriteDto> records, CancellationToken cancellationToken)
    {
        await attendance.UpsertRecordsAsync(sessionId, records, cancellationToken);
        return NoContent();
    }

    [HttpGet("classes/{classId:guid}/sessions")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Teacher}")]
    public async Task<ActionResult<IReadOnlyList<AttendanceSessionDto>>> ListSessions(Guid classId, [FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        return Ok(await attendance.ListSessionsAsync(classId, date, cancellationToken));
    }

    [HttpGet("students/{studentId:guid}/day-summaries")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.Parent},{RoleNames.Student}")]
    public async Task<ActionResult<IReadOnlyList<DayAttendanceSummaryDto>>> DaySummaries(
        Guid studentId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
    {
        return Ok(await attendance.GetStudentDaySummariesAsync(studentId, from, to, cancellationToken));
    }
}
