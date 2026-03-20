using Hangfire;
using Microsoft.EntityFrameworkCore;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Domain.Admin;
using OrdemCerta.Infrastructure.DataContext.Context;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.AdminRepository;
using Serilog;

namespace OrdemCerta.Presentation.Extensions;

public static class AppExtensions
{
    public static void UseApp(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerUi();
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
        }

        app.UseRateLimiter();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.UseHangfireDashboard("/hangfire", new DashboardOptions { Authorization = [] });
        app.ApplyMigrations();
        app.SeedAdmin();
    }

    public static void UseInfrastructure(this IApplicationBuilder app)
    {
        app.ApplyMigrations();
    }

    public static void AddSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrdemCerta API v1");
            options.RoutePrefix = "swagger";
        });
    }

    private static void UseSwaggerUi(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrdemCerta API v1");
            options.RoutePrefix = "swagger";
        });
    }

    private static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDataContext>();

        if (dbContext.Database.GetPendingMigrations().Any())
            dbContext.Database.Migrate();
    }

    private static void SeedAdmin(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var email = config["Admin:Email"];
        var password = config["Admin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var adminRepository = scope.ServiceProvider.GetRequiredService<IAdminRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        if (adminRepository.AnyAsync(CancellationToken.None).GetAwaiter().GetResult())
            return;

        var hash = passwordHasher.Hash(password);
        var admin = AdminUser.Create(email, hash);
        adminRepository.AddAsync(admin, CancellationToken.None).GetAwaiter().GetResult();
        unitOfWork.CommitAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}