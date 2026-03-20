using OrdemCerta.Presentation.Extensions;
using Serilog;

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

builder.WebHost.ConfigureKestrel(options => options.AllowSynchronousIO = true);

builder.Services.AddApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddMediatr();
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddWhatsApp(builder.Configuration);
builder.Services.AddMarketingJobs(builder.Configuration);
builder.Services.AddHangfire(builder.Configuration);
builder.Services.AddSwagger();
builder.Services.AddRateLimiting();

var app = builder.Build();

app.UseApp();
app.Run();
