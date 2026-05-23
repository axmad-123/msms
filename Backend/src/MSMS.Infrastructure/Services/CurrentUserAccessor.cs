using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MSMS.Application.Abstractions;
using MSMS.Domain.Constants;

namespace MSMS.Infrastructure.Services;

public sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Guid? UserId
    {
        get
        {
            var id = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var g) ? g : null;
        }
    }

    public IReadOnlyCollection<string> Roles =>
        httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();

    public bool IsInRole(string role) =>
        httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
}
