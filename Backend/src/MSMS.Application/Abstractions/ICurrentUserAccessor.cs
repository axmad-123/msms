namespace MSMS.Application.Abstractions;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}
