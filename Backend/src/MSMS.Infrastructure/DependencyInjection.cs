using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MSMS.Application.Abstractions;
using MSMS.Application.Configuration;
using MSMS.Infrastructure.Configuration;
using MSMS.Infrastructure.Identity;
using MSMS.Infrastructure.Persistence;
using MSMS.Infrastructure.Security;
using MSMS.Infrastructure.Services;

namespace MSMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));

        var connectionString = DatabaseConnection.Resolve(configuration);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");
        }

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPeopleAdminService, PeopleAdminService>();
        services.AddScoped<ISchoolStructureService, SchoolStructureService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IExamCatalogService, ExamCatalogService>();
        services.AddScoped<IExamResultService, ExamResultService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IStudentLifecycleService, StudentLifecycleService>();
        services.AddScoped<IParentPortalService, ParentPortalService>();
        services.AddScoped<IStudentPortalService, StudentPortalService>();
        services.AddScoped<ITeacherPortalService, TeacherPortalService>();
        services.AddScoped<IMediaStorageService, CloudinaryMediaService>();

        return services;
    }
}
