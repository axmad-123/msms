using MSMS.Application.DTOs.Attendance;

namespace MSMS.Application.Abstractions;

public interface IAttendanceService
{
    Task<Guid> CreateSessionAsync(CreateAttendanceSessionDto dto, CancellationToken cancellationToken = default);
    Task CloseSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task UpsertRecordsAsync(Guid sessionId, IReadOnlyList<AttendanceRecordWriteDto> records, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceSessionDto>> ListSessionsAsync(Guid classId, DateOnly? date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DayAttendanceSummaryDto>> GetStudentDaySummariesAsync(Guid studentId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
