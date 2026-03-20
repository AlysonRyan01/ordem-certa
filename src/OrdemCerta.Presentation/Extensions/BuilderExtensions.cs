using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrdemCerta.Application.Abstractions;
using OrdemCerta.Domain.Companies.Interfaces;
using OrdemCerta.Application.Services.AdminService;
using OrdemCerta.Application.Services.AuthService;
using OrdemCerta.Application.Services.CompanyService;
using OrdemCerta.Application.Services.CustomerService;
using OrdemCerta.Application.Services.DashboardService;
using OrdemCerta.Application.Services.PdfService;
using OrdemCerta.Application.Services.SaleService;
using OrdemCerta.Application.Services.ServiceOrderService;
using OrdemCerta.Application.Services.StripeService;
using OrdemCerta.Infrastructure.DataContext.Context;
using OrdemCerta.Infrastructure.DataContext.Uow;
using OrdemCerta.Infrastructure.Repositories.AdminRepository;
using OrdemCerta.Infrastructure.Repositories.CompanyRepository;
using OrdemCerta.Infrastructure.Repositories.CustomerRepository;
using OrdemCerta.Infrastructure.Repositories.SaleRepository;
using OrdemCerta.Infrastructure.Repositories.ServiceOrderRepository;
using OrdemCerta.Application.GooglePlaces;
using OrdemCerta.Application.Jobs;
using OrdemCerta.Application.Security;
using OrdemCerta.Application.WhatsApp;
using OrdemCerta.Infrastructure.Repositories.MarketingProspectRepository;

namespace OrdemCerta.Presentation.Extensions;

public static class BuilderExtensions
{
    public static void AddApi(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services.AddEndpointsApiExplorer();
    }

    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDatabase(services, configuration);
        AddRepositories(services);
        AddHealthChecks(services, configuration);
    }

    public static void AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
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
    }
    
    public static void AddServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentCompany, CurrentCompany>();

        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IServiceOrderService, ServiceOrderService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddMemoryCache();

        services.AddValidatorsFromAssemblyContaining<CustomerService>();
    }

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
            {
                Title = "OrdemCerta API",
                Version = "v1",
                Description = "API para gerenciamento de clientes"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o token JWT no formato: Bearer {token}"
            });

            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                }
            });
        });
    }

    private static void AddHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "postgresql",
                tags: ["db", "ready"]);
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDataContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDataContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
        services.AddScoped<ICompanyOrderSequenceRepository, CompanyOrderSequenceRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ICompanySaleSequenceRepository, CompanySaleSequenceRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IMarketingProspectRepository, MarketingProspectRepository>();
    }

    public static void AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey não configurado");

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "OrdemCerta",
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"] ?? "OrdemCerta",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("admin"));
        });
    }

    public static void AddMediatr(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(BuilderExtensions).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(ServiceOrderService).Assembly);
        });
    }

    public static void AddHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

        services.AddHangfireServer();
    }

    public static void AddMarketingJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IGooglePlacesService, GooglePlacesService>();
        services.AddScoped<MarketingProspectorJob>();
        services.AddScoped<MarketingDispatcherJob>();
    }

    public static void AddWhatsApp(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["EvolutionApi:BaseUrl"]
            ?? throw new InvalidOperationException("EvolutionApi:BaseUrl não configurado");

        var apiKey = configuration["EvolutionApi:ApiKey"]
            ?? throw new InvalidOperationException("EvolutionApi:ApiKey não configurado");

        services.AddHttpClient<IWhatsAppService, WhatsAppService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("apikey", apiKey);
        });
    }
}