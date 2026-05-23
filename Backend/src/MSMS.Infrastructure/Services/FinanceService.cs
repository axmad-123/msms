using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Finance;
using MSMS.Domain.Constants;
using MSMS.Domain.Entities;
using MSMS.Domain.Enums;
using MSMS.Infrastructure.Persistence;

namespace MSMS.Infrastructure.Services;

public sealed class FinanceService(ApplicationDbContext db, ICurrentUserAccessor currentUser) : IFinanceService
{
    public async Task<IReadOnlyList<MonthlyFeeDto>> ListFeesAsync(Guid? studentId, CancellationToken cancellationToken = default)
    {
        var query = db.MonthlyFees.AsNoTracking();

        if (currentUser.IsInRole(RoleNames.Parent))
        {
            var userId = currentUser.UserId ?? throw new InvalidOperationException();
            var parentId = await db.Parents.Where(p => p.UserId == userId).Select(p => p.Id).SingleAsync(cancellationToken);
            var childIds = await db.ParentChildren.Where(l => l.ParentId == parentId).Select(l => l.StudentId).ToListAsync(cancellationToken);

            if (studentId is not null && !childIds.Contains(studentId.Value))
            {
                throw new UnauthorizedAccessException("You can only view your linked students.");
            }

            query = query.Where(m => childIds.Contains(m.StudentId));
            if (studentId is not null)
            {
                query = query.Where(m => m.StudentId == studentId.Value);
            }
        }
        else if (currentUser.IsInRole(RoleNames.Admin))
        {
            if (studentId is not null)
            {
                query = query.Where(m => m.StudentId == studentId.Value);
            }
        }
        else
        {
            throw new UnauthorizedAccessException();
        }

        return await query
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Select(x => new MonthlyFeeDto(x.Id, x.StudentId, x.Year, x.Month, x.Amount, x.AcademicYear))
            .ToListAsync(cancellationToken);
    }

    public async Task<FinanceMonthSummaryDto> GetMonthSummaryAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        RequireAdmin();

        var rows = await db.Students.AsNoTracking()
            .Where(s => s.Status == StudentStatus.Active)
            .Select(s => new
            {
                s.Id,
                s.StudentNumber,
                StudentName = s.FirstName + " " + s.LastName,
                ClassName = s.Class != null ? s.Class.Name : null,
                FeeAmount = db.MonthlyFees
                    .Where(f => f.StudentId == s.Id && f.Year == year && f.Month == month)
                    .Select(f => (decimal?)f.Amount)
                    .FirstOrDefault() ?? 0m,
                PaidAmount = db.Payments
                    .Where(p => p.StudentId == s.Id && p.Year == year && p.Month == month && p.Status != PaymentStatus.Waived)
                    .Select(p => (decimal?)p.Amount)
                    .Sum() ?? 0m
            })
            .OrderBy(x => x.ClassName)
            .ThenBy(x => x.StudentName)
            .ToListAsync(cancellationToken);

        var students = rows.Select(x =>
        {
            var balance = Math.Max(0m, x.FeeAmount - x.PaidAmount);
            return new FinanceStudentMonthDto(
                x.Id,
                x.StudentNumber,
                x.StudentName,
                x.ClassName,
                year,
                month,
                x.FeeAmount,
                x.PaidAmount,
                balance,
                x.FeeAmount > 0 && balance <= 0);
        }).ToList();

        return new FinanceMonthSummaryDto(
            year,
            month,
            students.Count,
            students.Count(s => s.IsPaid),
            students.Count(s => !s.IsPaid),
            students.Sum(s => s.FeeAmount),
            students.Sum(s => s.PaidAmount),
            students.Sum(s => s.Balance),
            students);
    }

    public async Task<Guid> UpsertMonthlyFeeAsync(UpsertMonthlyFeeDto dto, CancellationToken cancellationToken = default)
    {
        RequireAdmin();

        var existing = await db.MonthlyFees.SingleOrDefaultAsync(
            x => x.StudentId == dto.StudentId && x.Year == dto.Year && x.Month == dto.Month && x.AcademicYear == dto.AcademicYear,
            cancellationToken);

        if (existing is null)
        {
            var entity = new MonthlyFee
            {
                Id = Guid.NewGuid(),
                StudentId = dto.StudentId,
                Year = dto.Year,
                Month = dto.Month,
                Amount = dto.Amount,
                AcademicYear = dto.AcademicYear.Trim()
            };

            db.MonthlyFees.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }

        existing.Amount = dto.Amount;
        await db.SaveChangesAsync(cancellationToken);
        return existing.Id;
    }

    public async Task DeleteMonthlyFeeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var entity = await db.MonthlyFees.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Monthly fee not found.");

        db.MonthlyFees.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentDto>> ListPaymentsAsync(Guid? studentId, CancellationToken cancellationToken = default)
    {
        var query = db.Payments.AsNoTracking();

        if (currentUser.IsInRole(RoleNames.Parent))
        {
            var userId = currentUser.UserId ?? throw new InvalidOperationException();
            var parentId = await db.Parents.Where(p => p.UserId == userId).Select(p => p.Id).SingleAsync(cancellationToken);
            var childIds = await db.ParentChildren.Where(l => l.ParentId == parentId).Select(l => l.StudentId).ToListAsync(cancellationToken);

            if (studentId is not null && !childIds.Contains(studentId.Value))
            {
                throw new UnauthorizedAccessException("You can only view your linked students.");
            }

            query = query.Where(m => childIds.Contains(m.StudentId));
            if (studentId is not null)
            {
                query = query.Where(m => m.StudentId == studentId.Value);
            }
        }
        else if (currentUser.IsInRole(RoleNames.Admin))
        {
            if (studentId is not null)
            {
                query = query.Where(m => m.StudentId == studentId.Value);
            }
        }
        else
        {
            throw new UnauthorizedAccessException();
        }

        return await query
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Select(x => new PaymentDto(
                x.Id,
                x.StudentId,
                x.Year,
                x.Month,
                x.Amount,
                x.PaymentDate,
                x.Status,
                x.RecordedByAdminId,
                x.Notes,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var adminId = await GetAdministratorIdAsync(cancellationToken);

        var entity = new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = dto.StudentId,
            Year = dto.Year,
            Month = dto.Month,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate,
            Status = dto.Status,
            RecordedByAdminId = adminId,
            Notes = dto.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Payments.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task UpdatePaymentAsync(Guid id, UpdatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var entity = await db.Payments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Payment not found.");

        entity.Year = dto.Year;
        entity.Month = dto.Month;
        entity.Amount = dto.Amount;
        entity.PaymentDate = dto.PaymentDate;
        entity.Status = dto.Status;
        entity.Notes = dto.Notes;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var entity = await db.Payments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Payment not found.");

        db.Payments.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> ChargeMonthlyFeeAsync(Guid studentId, int year, int month, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var adminId = await GetAdministratorIdAsync(cancellationToken);

        var fee = await db.MonthlyFees
            .Where(f => f.StudentId == studentId && f.Year == year && f.Month == month)
            .OrderByDescending(f => f.AcademicYear)
            .FirstOrDefaultAsync(cancellationToken);

        if (fee is null)
        {
            var latestFee = await db.MonthlyFees.AsNoTracking()
                .Where(f => f.StudentId == studentId)
                .OrderByDescending(f => f.Year)
                .ThenByDescending(f => f.Month)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("No monthly fee defined for this student.");

            fee = new MonthlyFee
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                Year = year,
                Month = month,
                Amount = latestFee.Amount,
                AcademicYear = latestFee.AcademicYear
            };
            db.MonthlyFees.Add(fee);
        }

        var exists = await db.Payments.AnyAsync(
            p => p.StudentId == studentId && p.Year == year && p.Month == month && p.Status == PaymentStatus.Paid,
            cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("This month is already paid.");
        }

        var entity = new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            Year = year,
            Month = month,
            Amount = fee.Amount,
            PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = PaymentStatus.Paid,
            RecordedByAdminId = adminId,
            Notes = $"Monthly fee {month}/{year}",
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Payments.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    private void RequireAdmin()
    {
        if (!currentUser.IsInRole(RoleNames.Admin))
        {
            throw new UnauthorizedAccessException("Administrator access is required.");
        }
    }

    private async Task<Guid> GetAdministratorIdAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException();
        return await db.Administrators.Where(a => a.UserId == userId).Select(a => a.Id).SingleAsync(cancellationToken);
    }
}
