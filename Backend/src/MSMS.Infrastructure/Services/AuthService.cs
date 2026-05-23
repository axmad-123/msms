using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSMS.Application.Abstractions;
using MSMS.Application.DTOs.Auth;
using MSMS.Domain.Constants;
using MSMS.Domain.Entities;
using MSMS.Infrastructure.Identity;
using MSMS.Infrastructure.Persistence;
using MSMS.Infrastructure.Security;

namespace MSMS.Infrastructure.Services;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ApplicationDbContext db,
    JwtTokenService jwtTokenService) : IAuthService
{
    public async Task<TokenResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var valid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var roles = await userManager.GetRolesAsync(user);
        return await IssueTokensAsync(user, roles, cancellationToken);
    }

    public async Task<TokenResponseDto> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await db.RefreshTokens
            .SingleOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken);

        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var user = await userManager.FindByIdAsync(existing.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        existing.RevokedAtUtc = DateTime.UtcNow;
        var roles = await userManager.GetRolesAsync(user);
        var response = await IssueTokensAsync(user, roles, cancellationToken);
        existing.ReplacedByToken = response.RefreshToken;
        await db.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task RegisterUserAsync(RegisterUserRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        if (!await roleManager.RoleExistsAsync(request.Role))
        {
            throw new InvalidOperationException($"Unknown role '{request.Role}'.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, request.Role);

        switch (request.Role)
        {
            case RoleNames.Student:
                if (string.IsNullOrWhiteSpace(request.StudentNumber) ||
                    string.IsNullOrWhiteSpace(request.StudentFirstName) ||
                    string.IsNullOrWhiteSpace(request.StudentLastName))
                {
                    throw new InvalidOperationException("Student profile fields are required.");
                }

                db.Students.Add(new Student
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    StudentNumber = request.StudentNumber.Trim(),
                    FirstName = request.StudentFirstName.Trim(),
                    LastName = request.StudentLastName.Trim(),
                    DateOfBirth = request.StudentDateOfBirth,
                    ClassId = request.StudentClassId,
                    CreatedAtUtc = DateTime.UtcNow
                });
                break;

            case RoleNames.Parent:
                if (string.IsNullOrWhiteSpace(request.ParentFirstName) || string.IsNullOrWhiteSpace(request.ParentLastName))
                {
                    throw new InvalidOperationException("Parent profile fields are required.");
                }

                db.Parents.Add(new Parent
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FirstName = request.ParentFirstName.Trim(),
                    LastName = request.ParentLastName.Trim(),
                    Phone = request.ParentPhone,
                    CreatedAtUtc = DateTime.UtcNow
                });
                break;

            case RoleNames.Teacher:
                if (string.IsNullOrWhiteSpace(request.TeacherEmployeeNumber) ||
                    string.IsNullOrWhiteSpace(request.TeacherFirstName) ||
                    string.IsNullOrWhiteSpace(request.TeacherLastName))
                {
                    throw new InvalidOperationException("Teacher profile fields are required.");
                }

                db.Teachers.Add(new Teacher
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    EmployeeNumber = request.TeacherEmployeeNumber.Trim(),
                    FirstName = request.TeacherFirstName.Trim(),
                    LastName = request.TeacherLastName.Trim(),
                    CreatedAtUtc = DateTime.UtcNow
                });
                break;

            case RoleNames.Admin:
                db.Administrators.Add(new Administrator
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    DisplayName = string.IsNullOrWhiteSpace(request.AdminDisplayName) ? request.FullName : request.AdminDisplayName!.Trim(),
                    CreatedAtUtc = DateTime.UtcNow
                });
                break;

            default:
                throw new InvalidOperationException("Unsupported role for registration.");
        }

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private async Task<TokenResponseDto> IssueTokensAsync(ApplicationUser user, IList<string> roles, CancellationToken cancellationToken)
    {
        var access = jwtTokenService.CreateAccessToken(user, roles);
        var refreshValue = jwtTokenService.GenerateRefreshTokenValue();
        var refreshEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshValue,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = jwtTokenService.GetRefreshTokenExpiryUtc()
        };

        db.RefreshTokens.Add(refreshEntity);
        await db.SaveChangesAsync(cancellationToken);

        return new TokenResponseDto(
            access,
            refreshValue,
            jwtTokenService.GetAccessTokenExpiryUtc(),
            refreshEntity.ExpiresAtUtc,
            roles.ToArray());
    }
}
