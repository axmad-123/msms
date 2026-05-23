using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Attendance;
using MSMS.Domain.Constants;
using MSMS.Domain.Entities;
using MSMS.Domain.Enums;
using MSMS.Infrastructure.Persistence;

namespace MSMS.Infrastructure.Services;

public sealed class AttendanceService(ApplicationDbContext db, ICurrentUserAccessor currentUser) : IAttendanceService
{
    public async Task<Guid> CreateSessionAsync(CreateAttendanceSessionDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.SessionNumber is < 1 or > 2)
        {
            throw new InvalidOperationException("SessionNumber must be 1 or 2.");
        }

        var adminUserId = currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");
        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new UnauthorizedAccessException("Only administrators can start attendance sessions.");
        }

        var classExists = await db.Classes.AnyAsync(c => c.Id == dto.ClassId, cancellationToken);
        if (!classExists)
        {
            throw new InvalidOperationException("Class not found.");
        }

        var session = new AttendanceSession
        {
            Id = Guid.NewGuid(),
            ClassId = dto.ClassId,
            SessionDate = dto.SessionDate,
            SessionNumber = dto.SessionNumber,
            StartedByUserId = adminUserId,
            Status = AttendanceSessionStatus.Open,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.AttendanceSessions.Add(session);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("A session already exists for this class, date, and session number.");
        }

        return session.Id;
    }

    public async Task CloseSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new UnauthorizedAccessException("Only administrators can close attendance sessions.");
        }

        var session = await db.AttendanceSessions.SingleOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            ?? throw new InvalidOperationException("Session not found.");

        session.Status = AttendanceSessionStatus.Closed;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertRecordsAsync(Guid sessionId, IReadOnlyList<AttendanceRecordWriteDto> records, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsInRole(RoleNames.Teacher))
        {
            throw new UnauthorizedAccessException("Only teachers can record attendance.");
        }

        var userId = currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");
        var teacher = await db.Teachers.SingleOrDefaultAsync(t => t.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Teacher profile not found.");

        var session = await db.AttendanceSessions.Include(s => s.Class).SingleOrDefaultAsync(s => s.Id == sessionId, cancellationToken)
            ?? throw new InvalidOperationException("Session not found.");

        if (session.Status != AttendanceSessionStatus.Open)
        {
            throw new InvalidOperationException("Attendance session is closed.");
        }

        var allowed = await db.TeacherSubjects.AnyAsync(ts => ts.TeacherId == teacher.Id && ts.ClassId == session.ClassId, cancellationToken);
        if (!allowed)
        {
            throw new UnauthorizedAccessException("You are not assigned to this class.");
        }

        var studentIds = records.Select(r => r.StudentId).Distinct().ToArray();
        var validCount = await db.Students.CountAsync(
            s => studentIds.Contains(s.Id) && s.ClassId == session.ClassId && s.Status == StudentStatus.Active,
            cancellationToken);

        if (validCount != studentIds.Length)
        {
            throw new InvalidOperationException("One or more students are not active members of this class.");
        }

        var existing = await db.AttendanceRecords.Where(r => r.AttendanceSessionId == sessionId).ToListAsync(cancellationToken);
        foreach (var dto in records)
        {
            var mark = dto.Mark == 0 ? AttendanceMark.Present : AttendanceMark.Absent;
            var row = existing.SingleOrDefault(r => r.StudentId == dto.StudentId);
            if (row is null)
            {
                db.AttendanceRecords.Add(new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    AttendanceSessionId = sessionId,
                    StudentId = dto.StudentId,
                    RecordedByTeacherId = teacher.Id,
                    Mark = mark,
                    RecordedAtUtc = DateTime.UtcNow
                });
            }
            else
            {
                row.Mark = mark;
                row.RecordedByTeacherId = teacher.Id;
                row.RecordedAtUtc = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AttendanceSessionDto>> ListSessionsAsync(Guid classId, DateOnly? date, CancellationToken cancellationToken = default)
    {
        if (!(currentUser.IsInRole(RoleNames.Admin) || currentUser.IsInRole(RoleNames.Teacher)))
        {
            throw new UnauthorizedAccessException();
        }

        if (currentUser.IsInRole(RoleNames.Teacher))
        {
            var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();
            var teacherId = await db.Teachers.Where(t => t.UserId == userId).Select(t => t.Id).SingleAsync(cancellationToken);
            var allowed = await db.TeacherSubjects.AnyAsync(ts => ts.TeacherId == teacherId && ts.ClassId == classId, cancellationToken);
            if (!allowed)
            {
                throw new UnauthorizedAccessException();
            }
        }

        var query = db.AttendanceSessions.AsNoTracking().Where(s => s.ClassId == classId);
        if (date is not null)
        {
            query = query.Where(s => s.SessionDate == date.Value);
        }

        return await query
            .OrderByDescending(s => s.SessionDate).ThenBy(s => s.SessionNumber)
            .Select(s => new AttendanceSessionDto(s.Id, s.ClassId, s.SessionDate, s.SessionNumber, s.Status.ToString(), s.StartedByUserId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DayAttendanceSummaryDto>> GetStudentDaySummariesAsync(Guid studentId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var student = await db.Students.AsNoTracking().SingleOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            ?? throw new InvalidOperationException("Student not found.");

        await EnsureCanViewStudentAttendanceAsync(studentId, cancellationToken);

        if (student.ClassId is null)
        {
            return Array.Empty<DayAttendanceSummaryDto>();
        }

        var classId = student.ClassId.Value;
        var sessions = await db.AttendanceSessions.AsNoTracking()
            .Where(s => s.ClassId == classId && s.SessionDate >= from && s.SessionDate <= to)
            .ToListAsync(cancellationToken);

        var sessionIds = sessions.Select(s => s.Id).ToArray();
        var records = await db.AttendanceRecords.AsNoTracking()
            .Where(r => r.StudentId == studentId && sessionIds.Contains(r.AttendanceSessionId))
            .ToListAsync(cancellationToken);

        var result = new List<DayAttendanceSummaryDto>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            var daySessions = sessions.Where(s => s.SessionDate == d).OrderBy(s => s.SessionNumber).ToArray();
            if (daySessions.Length == 0)
            {
                continue;
            }

            int? s1 = null;
            int? s2 = null;
            var s1Entity = daySessions.FirstOrDefault(x => x.SessionNumber == 1);
            var s2Entity = daySessions.FirstOrDefault(x => x.SessionNumber == 2);
            if (s1Entity is not null)
            {
                var r1 = records.SingleOrDefault(r => r.AttendanceSessionId == s1Entity.Id);
                s1 = r1 is null ? null : (int)r1.Mark;
            }

            if (s2Entity is not null)
            {
                var r2 = records.SingleOrDefault(r => r.AttendanceSessionId == s2Entity.Id);
                s2 = r2 is null ? null : (int)r2.Mark;
            }

            var outcome = ComputeOutcome(s1, s2, s1Entity is not null, s2Entity is not null);
            result.Add(new DayAttendanceSummaryDto(d, s1, s2, outcome));
        }

        return result;
    }

    private async Task EnsureCanViewStudentAttendanceAsync(Guid studentId, CancellationToken cancellationToken)
    {
        if (currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        var userId = currentUser.UserId ?? throw new UnauthorizedAccessException();

        if (currentUser.IsInRole(RoleNames.Student))
        {
            var ok = await db.Students.AnyAsync(s => s.Id == studentId && s.UserId == userId, cancellationToken);
            if (!ok)
            {
                throw new UnauthorizedAccessException();
            }

            return;
        }

        if (currentUser.IsInRole(RoleNames.Parent))
        {
            var parentId = await db.Parents.Where(p => p.UserId == userId).Select(p => p.Id).SingleAsync(cancellationToken);
            var ok = await db.ParentChildren.AnyAsync(l => l.ParentId == parentId && l.StudentId == studentId, cancellationToken);
            if (!ok)
            {
                throw new UnauthorizedAccessException();
            }

            return;
        }

        throw new UnauthorizedAccessException();
    }

    private static string ComputeOutcome(int? s1, int? s2, bool hasSession1, bool hasSession2)
    {
        if (hasSession1 && hasSession2)
        {
            var a1 = s1 == (int)AttendanceMark.Absent;
            var a2 = s2 == (int)AttendanceMark.Absent;
            var p1 = s1 == (int)AttendanceMark.Present;
            var p2 = s2 == (int)AttendanceMark.Present;

            if (a1 && a2)
            {
                return DayAttendanceOutcome.FullAbsent.ToString();
            }

            if (p1 && p2)
            {
                return DayAttendanceOutcome.Present.ToString();
            }

            return DayAttendanceOutcome.PartialPresent.ToString();
        }

        if (hasSession1 && !hasSession2)
        {
            return s1 == (int)AttendanceMark.Absent
                ? DayAttendanceOutcome.FullAbsent.ToString()
                : DayAttendanceOutcome.PartialPresent.ToString();
        }

        if (!hasSession1 && hasSession2)
        {
            return s2 == (int)AttendanceMark.Absent
                ? DayAttendanceOutcome.FullAbsent.ToString()
                : DayAttendanceOutcome.PartialPresent.ToString();
        }

        return DayAttendanceOutcome.PartialPresent.ToString();
    }
}
