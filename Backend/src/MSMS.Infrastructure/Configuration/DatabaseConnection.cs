using Microsoft.Extensions.Configuration;

namespace MSMS.Infrastructure.Configuration;

public static class DatabaseConnection
{
    public static string Resolve(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            try
            {
                return FromDatabaseUrl(databaseUrl);
            }
            catch (UriFormatException)
            {
                // Fall back to appsettings when DATABASE_URL is malformed on the host.
            }
        }

        return configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    private static string FromDatabaseUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/').Split('?')[0];
        var port = uri.Port > 0 ? uri.Port : 5432;

        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
