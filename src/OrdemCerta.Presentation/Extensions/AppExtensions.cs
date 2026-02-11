using OrdemCerta.Infrastructure.DataContext.Context;
using Microsoft.EntityFrameworkCore;

namespace OrdemCerta.Presentation.Extensions;

public static class AppExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDataContext>();
        
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }
    }

    public static void UseInfrastructure(this IApplicationBuilder app)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            app.ApplyMigrations();
        }
    }
}