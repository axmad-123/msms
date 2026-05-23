using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using MSMS.Api.Middleware;
using MSMS.Application;
using MSMS.Infrastructure;
using MSMS.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Modern School Management System API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
var corsEnv = Environment.GetEnvironmentVariable("CORS_ORIGINS");
if (!string.IsNullOrWhiteSpace(corsEnv))
{
    allowedOrigins = corsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
builder.Services.AddCors(policy =>
{
    policy.AddPolicy(
        "Default",
        configure =>
        {
            if (allowedOrigins.Length > 0)
            {
                configure.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
            }
            else
            {
                configure.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            }
        });
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

var enableSwagger = app.Environment.IsDevelopment()
    || builder.Configuration.GetValue("EnableSwagger", false);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
await DatabaseSeeder.SeedAsync(app.Services);

await app.RunAsync();

public partial class Program
{
}
