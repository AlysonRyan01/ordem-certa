using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Hangfire;
using Microsoft.AspNetCore.RateLimiting;
using OrdemCerta.Presentation.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            "logs/ordemcerta-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30));

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    builder.WebHost.ConfigureKestrel(options =>
        options.AllowSynchronousIO = true);
    builder.Services.AddServices();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddMediatr();
    builder.Services.AddAuth(builder.Configuration);
    builder.Services.AddWhatsApp(builder.Configuration);
    builder.Services.AddHangfire(builder.Configuration);
    builder.Services.AddSwagger();
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "postgresql",
            tags: ["db", "ready"]);

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("public", limiter =>
        {
            limiter.PermitLimit = 10;
            limiter.Window = TimeSpan.FromMinutes(1);
            limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiter.QueueLimit = 0;
        });

        options.AddPolicy("per-company", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User.FindFirst("companyId")?.Value
                              ?? httpContext.Connection.RemoteIpAddress?.ToString()
                              ?? "anonymous",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                }));

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.AddSwagger();
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
    }

    app.UseRateLimiter();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.Use(async (context, next) =>
    {
        context.Request.EnableBuffering();
        await next();
    });

    app.MapControllers();
    app.MapHealthChecks("/health");
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [],
    });
    app.UseInfrastructure();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
