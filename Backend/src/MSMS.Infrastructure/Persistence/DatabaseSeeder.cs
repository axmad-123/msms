using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;

using MSMS.Domain.Constants;

using MSMS.Domain.Entities;

using MSMS.Infrastructure.Identity;



namespace MSMS.Infrastructure.Persistence;



public static class DatabaseSeeder
{
    /// <summary>Runs migrations and ensures admin/roles exist before the web server starts.</summary>
    public static async Task PrepareAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = provider.GetRequiredService<ApplicationDbContext>();

        await db.Database.MigrateAsync(cancellationToken);



        foreach (var role in new[] { RoleNames.Admin, RoleNames.Teacher, RoleNames.Parent, RoleNames.Student })

        {

            if (await roleManager.RoleExistsAsync(role))

            {

                continue;

            }



            var created = await roleManager.CreateAsync(new IdentityRole<Guid>(role));

            if (!created.Succeeded)

            {

                throw new InvalidOperationException($"Failed to create role '{role}'.");

            }

        }



        const string adminEmail = "admin@msms.local";

        if (await userManager.FindByEmailAsync(adminEmail) is null)

        {

            var adminUser = new ApplicationUser

            {

                Id = Guid.NewGuid(),

                UserName = adminEmail,

                Email = adminEmail,

                FullName = "System Administrator",

                EmailConfirmed = true,

                IsActive = true,

                CreatedAtUtc = DateTime.UtcNow,

            };



            var userResult = await userManager.CreateAsync(adminUser, "ChangeMe!12345");

            if (!userResult.Succeeded)

            {

                throw new InvalidOperationException(string.Join("; ", userResult.Errors.Select(e => e.Description)));

            }



            await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);



            db.Administrators.Add(new Administrator

            {

                Id = Guid.NewGuid(),

                UserId = adminUser.Id,

                DisplayName = "System Administrator",

                CreatedAtUtc = DateTime.UtcNow,

            });



            await db.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>Heavy demo dataset — run in background so Railway healthchecks pass quickly.</summary>
    public static Task SeedDemoAsync(IServiceProvider services, CancellationToken cancellationToken = default) =>
        DemoDataSeeder.SeedAsync(services, cancellationToken);

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await PrepareAsync(services, cancellationToken);
        await SeedDemoAsync(services, cancellationToken);
    }
}


