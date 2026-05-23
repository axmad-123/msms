namespace MSMS.Application.DTOs.Attendance;

public sealed record CreateAttendanceSessionDto(Guid ClassId, DateOnly SessionDate, int SessionNumber);

public sealed record AttendanceSessionDto(
    Guid Id,
    Guid ClassId,
    DateOnly SessionDate,
    int SessionNumber,
    string Status,
    Guid StartedByUserId);

public sealed record AttendanceRecordWriteDto(Guid StudentId, int Mark);

public sealed record DayAttendanceSummaryDto(
    DateOnly Date,
    int? Session1Mark,
    int? Session2Mark,
    string Outcome);
