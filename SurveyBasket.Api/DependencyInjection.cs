using Asp.Versioning;
using FluentValidation.AspNetCore;
using Hangfire;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SurveyBasket.Api.Authentication.Filters;
using SurveyBasket.Api.Entities;
using SurveyBasket.Api.Errors;
using SurveyBasket.Api.Extensions;
using SurveyBasket.Api.Health;
using SurveyBasket.Api.Services.ClassServices;
using SurveyBasket.Api.Services.InterfaceServices;
using SurveyBasket.Api.Settings;
using SurveyBasket.Api.Swagger;
using SurveyBasket.Authentication;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

namespace SurveyBasket.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services,
        IConfiguration configuration, IHostBuilder host)
    {
        services.AddControllers();
        services.AddCacheServices();

        services.AddCorsServices(configuration);
        services.AddAuthConfig(configuration);


        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddSwaggerServices()
            .AddMapsterConfig()
            .AddFluentValidationConfig();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddHealthChecksServices(connectionString);
        services.AddRateLimiterServices();

        services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<INotificationService, NotificationService>();
        //services.AddScoped<ICacheService, CacheService>();


        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddHttpContextAccessor();
        services.AddBackgroundJobsConfig(configuration);
        host.AddSerilogServices();



        return services;
    }

    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            //options.SwaggerDoc("v1", new OpenApiInfo
            //{
            //    Version = "v1",
            //    Title = "ToDo API",
            //    Description = "An ASP.NET Core Web API for managing ToDo items",
            //    TermsOfService = new Uri("https://example.com/terms"),
            //    Contact = new OpenApiContact
            //    {
            //        Name = "Example Contact",
            //        Url = new Uri("https://example.com/contact")
            //    },
            //    License = new OpenApiLicense
            //    {
            //        Name = "Example License",
            //        Url = new Uri("https://example.com/license")
            //    }
            //});

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            options.OperationFilter<SwaggerDefaultValues>();
        });

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        return services;
    }
    private static IServiceCollection AddRateLimiterServices(this IServiceCollection services)
    {
        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiterOptions.AddPolicy("ipLimit", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 2,
                        Window = TimeSpan.FromSeconds(20)
                    }
                )
            );

            rateLimiterOptions.AddPolicy("userLimit", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.GetUserId(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 2,
                        Window = TimeSpan.FromSeconds(20)
                    }
                )
            );

            rateLimiterOptions.AddConcurrencyLimiter("concurrency", options =>
            {
                options.PermitLimit = 1000;
                options.QueueLimit = 100;
                options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            });

            //rateLimiterOptions.AddTokenBucketLimiter("token", options =>
            //{
            //    options.TokenLimit = 2;
            //    options.QueueLimit = 1;
            //    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            //    options.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            //    options.TokensPerPeriod = 2;
            //    options.AutoReplenishment = true;
            //});

            //rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
            //{
            //    options.PermitLimit = 2;
            //    options.QueueLimit = 1;
            //    options.Window = TimeSpan.FromSeconds(20);
            //    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            //});

            //rateLimiterOptions.AddSlidingWindowLimiter("sliding", options =>
            //{
            //    options.PermitLimit = 2;
            //    options.QueueLimit = 1;
            //    options.SegmentsPerWindow = 2;
            //    options.Window = TimeSpan.FromSeconds(20);
            //    options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            //});
        });

        return services;
    }
    private static IServiceCollection AddHealthChecksServices(this IServiceCollection services, string connectionString)
    {
        services.AddHealthChecks()
             .AddSqlServer(connectionString, name: "DataBase")
             .AddHangfire(options => { options.MinimumAvailableServers = 1; }, name: "Hangfire")
             .AddUrlGroup(uri: new Uri("https://www.google.com"), name: "External Google", tags: ["api"], httpMethod: HttpMethod.Get)
             .AddUrlGroup(uri: new Uri("https://www.facebook.com"), name: "External FaceBook", tags: ["api"])
             .AddCheck<MailProviderHealthCheck>("Mail");

        return services;
    }
    private static IServiceCollection AddCacheServices(this IServiceCollection services)
    {
        services.AddHybridCache();

        //services.AddDistributedMemoryCache();

        //services.AddMemoryCache();

        //builder.Services.AddOutputCache(options =>
        //{
        //    options.AddPolicy("CachePolicy", s =>
        //        s
        //        .Cache()
        //        .Expire(TimeSpan.FromSeconds(120))
        //        .Tag("availableQuestion")
        //    );
        //});

        return services;
    }
    private static IHostBuilder AddSerilogServices(this IHostBuilder host)
    {
        //builder.Host.UseSerilog((context, configuration) =>
        //{
        //    configuration
        //        .MinimumLevel.Information()
        //        .WriteTo.Console();
        //});

        host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
        });

        return host;
    }
    private static IServiceCollection AddMapsterConfig(this IServiceCollection services) 
    {
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton<IMapper>(new Mapper(mappingConfig));

        return services;
    }
    private static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
    {
        var getOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()!;

        services.AddCors(options =>
            options.AddDefaultPolicy(builder =>
                builder.AllowAnyHeader()
                       .AllowAnyMethod()
                       .WithOrigins(getOrigins)
            )
        );

        //services.AddCors(options =>
        //    options.AddPolicy("MyPolicy", builder =>
        //                builder.AllowAnyOrigin()
        //                       .AllowAnyMethod()
        //                       .AllowAnyHeader()
        //                       //.WithOrigins("https://localhost:5222")
        //                       //.WithMethods("Get", "Post")
        //    )
        //);

        return services;
    }
    private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
    {
        services
            .AddFluentValidationAutoValidation().AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
    private static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        services.AddSingleton<IJwtProvider, JwtProvider>();

        //services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.key!)),
                ValidIssuer = jwtSettings?.Issuer,
                ValidAudience = jwtSettings?.Audience
            };
        }); 
        
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;
        });

        return services;
    }
    private static IServiceCollection AddBackgroundJobsConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Hangfire services.
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

        // Add the processing server as IHostedService
        services.AddHangfireServer();

        return services;
    }


}