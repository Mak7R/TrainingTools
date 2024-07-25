using System.Globalization;
using System.Text;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Application.Services;
using Application.Services.ReferencedContentProviders;
using Domain.Identity;
using Domain.Models;
using Domain.Models.Friendship;
using Domain.Models.TrainingPlan;
using Infrastructure.Data;
using Infrastructure.Mapping.Profiles;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebUI.Mapping.Profiles;
using WebUI.ModelBinding.Providers;
using WebUI.Policies.Handlers;
using WebUI.Policies.Requirements;

namespace WebUI.Extensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        
        var activeConnection = configuration["ActiveConnection"] ?? "DefaultConnection";
        var sqlServerConnectionString = configuration.GetConnectionString(activeConnection) ?? throw new InvalidOperationException($"Connection string '{activeConnection}' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(sqlServerConnectionString)
        );

        services.AddHealthChecks()
            .AddSqlServer(sqlServerConnectionString, name: "SQL Server");

        services.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(0, new UpdateTrainingPlanModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new FilterModelBinderProvider());

                // option.Filters
            })
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization();
            //.AddRazorRuntimeCompilation();

        services.AddHttpContextAccessor();
        services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("uk-UA")
            };

            options.DefaultRequestCulture = new RequestCulture("en-US");

            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            
            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new CookieRequestCultureProvider()
            };

            options.ApplyCurrentCultureToResponseHeaders = true;
        });

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;

                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.!#$%^&*()_-+=";
                options.User.RequireUniqueEmail = true;
            })
            .AddUserManager<MsSqlSpecificUserManager>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>()
            .AddDefaultTokenProviders();


        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
        });

        services.AddAuthentication()
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.HttpOnly = true;
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.AccessDeniedPath = "/access-denied";
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"] ??
                        throw new NullReferenceException("SecretKey can't be null")))
                };

                if (string.IsNullOrWhiteSpace(configuration["Jwt:Audience"]))
                {
                    tokenValidationParameters.ValidateAudience = false;
                }
                else
                {
                    tokenValidationParameters.ValidateAudience = true;
                    tokenValidationParameters.ValidAudience = configuration["Jwt:Audience"];
                }

                if (string.IsNullOrWhiteSpace(configuration["Jwt:Issuer"]))
                {
                    tokenValidationParameters.ValidateIssuer = false;
                }
                else
                {
                    tokenValidationParameters.ValidateIssuer = true;
                    tokenValidationParameters.ValidIssuer = configuration["Jwt:Issuer"];
                }
                
                
                options.TokenValidationParameters = tokenValidationParameters;
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(nameof(VerifyClaimsRequirement), policy => policy.AddRequirements(new VerifyClaimsRequirement()));
        
        services.AddApiVersioning(config =>
        {
            config.ApiVersionReader = new UrlSegmentApiVersionReader();
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>() ?? throw new InvalidOperationException("AllowedOrigins was not found"));
                builder.WithHeaders(configuration.GetSection("AllowedHeaders").Get<string[]>() ?? throw new InvalidOperationException("AllowedHeaders was not found"));
                builder.WithMethods(configuration.GetSection("AllowedMethods").Get<string[]>() ?? throw new InvalidOperationException("AllowedMethods was not found"));
            });
        });

        // setup swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => {
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api-docs.xml"));
            
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,

                    },
                    new List<string>()
                }
            });
            
            options.SwaggerDoc("v1", new OpenApiInfo{ Title = "Training Tools Web API V1", Version = "1.0" });
        });
        services.AddVersionedApiExplorer(options => {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddHttpClient();
        services.AddAutoMapper(typeof(UiApplicationMappingProfile), typeof(InfrastructureApplicationMappingProfile));

        services.AddApplicationServices(configuration);

        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode;
        });
        
        return services;
    }
    
    private static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRepository<Group, Guid>, GroupsRepository>();
        services.AddScoped<IRepository<Exercise, Guid>, ExercisesRepository>();
        services.AddScoped<IRepository<TrainingPlan, Guid>, TrainingPlansRepository>();
        services.AddScoped<IRepository<FriendInvitation, (Guid, Guid)>, FriendInvitationsRepository>();
        services.AddScoped<IRepository<Friendship, (Guid, Guid)>, FriendshipsRepository>();
        services.AddScoped<IRepository<ExerciseResult, (Guid, Guid)>, ExerciseResultsRepository>();
        
        services.AddScoped<IGroupsService, GroupsService>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IFriendsService, FriendshipsService>();
        services.AddScoped<IExercisesService, ExercisesService>();
        services.AddScoped<IExerciseResultsService, ExerciseResultsService>();
        services.AddScoped<ITrainingPlansService, TrainingPlansService>();
        
        services.AddScoped<IReferencedContentProvider, ImagesAndVideosReferencedContentProvider>();
        services.AddSingleton<IAuthTokenService<TokenGenerationInfo>, JwtService>();
        services.AddSingleton<IExerciseResultsToExсelExporter, ExerciseResultsToExcelExporter>();
        
        services.AddScoped<IAuthorizationHandler, VerifyClaimsRequirementHandler>();
    }
}