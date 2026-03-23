using FluentValidation.AspNetCore;
using Hangfire;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SurveyBasket.Api.Errors;
using SurveyBasket.Api.Services.ClassServices;
using SurveyBasket.Api.Services.InterfaceServices;
using SurveyBasket.Api.Settings;
using SurveyBasket.Authentication;
using System.Reflection;
using System.Text;

namespace SurveyBasket;

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

        services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<IUserService, UserService>();
        //services.AddScoped<ICacheService, CacheService>();

        services.AddHttpContextAccessor();
        services.AddBackgroundJobsConfig(configuration);
        host.AddSerilogServices();

        return services;
    }

    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

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
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
    private static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

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