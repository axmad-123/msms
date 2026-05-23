using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Exams;
using MSMS.Domain.Entities;
using MSMS.Domain.Enums;
using MSMS.Infrastructure.Persistence;

namespace MSMS.Infrastructure.Services;

public sealed class ExamCatalogService(ApplicationDbContext db) : IExamCatalogService
{
    public async Task<IReadOnlyList<ExamDto>> ListAsync(string? academicYear, CancellationToken cancellationToken = default)
    {
        var query = db.Exams.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(academicYear))
        {
            query = query.Where(e => e.AcademicYear == academicYear);
        }

        return await query
            .OrderBy(e => e.ExamType)
            .Select(e => new ExamDto(e.Id, (int)e.ExamType, e.Name, e.AcademicYear))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(CreateExamDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(ExamType), dto.ExamType))
        {
            throw new InvalidOperationException("Invalid exam type.");
        }

        var entity = new Exam
        {
            Id = Guid.NewGuid(),
            ExamType = (ExamType)dto.ExamType,
            Name = dto.Name.Trim(),
            AcademicYear = dto.AcademicYear.Trim()
        };

        db.Exams.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task UpdateAsync(Guid id, CreateExamDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(ExamType), dto.ExamType))
        {
            throw new InvalidOperationException("Invalid exam type.");
        }

        var entity = await db.Exams.SingleOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Exam not found.");

        entity.ExamType = (ExamType)dto.ExamType;
        entity.Name = dto.Name.Trim();
        entity.AcademicYear = dto.AcademicYear.Trim();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await db.Exams.SingleOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Exam not found.");

        db.Exams.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}
