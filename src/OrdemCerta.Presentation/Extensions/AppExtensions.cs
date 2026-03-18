using Hangfire;
using Microsoft.EntityFrameworkCore;
using OrdemCerta.Infrastructure.DataContext.Context;
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
}