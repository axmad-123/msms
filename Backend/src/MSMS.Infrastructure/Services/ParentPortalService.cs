using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Parent;
using MSMS.Domain.Constants;
using MSMS.Infrastructure.Persistence;

namespace MSMS.Infrastructure.Services;

public sealed class ParentPortalService(ApplicationDbContext db, ICurrentUserAccessor currentUser) : IParentPortalService
{
    public async Task<IReadOnlyList<LinkedStudentDto>> GetLinkedStudentsAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsInRole(RoleNames.Parent))
        {
            throw new UnauthorizedAccessException();
        }

        var userId = currentUser.UserId ?? throw new InvalidOperationException();
        var parentId = await db.Parents.Where(p => p.UserId == userId).Select(p => p.Id).SingleAsync(cancellationToken);

        return await db.ParentChildren.AsNoTracking()
            .Where(l => l.ParentId == parentId)
            .Join(db.Students, l => l.StudentId, s => s.Id, (l, s) => s)
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            .Select(s => new LinkedStudentDto(s.Id, s.StudentNumber, s.FirstName, s.LastName, s.ClassId))
            .ToListAsync(cancellationToken);
    }
}
