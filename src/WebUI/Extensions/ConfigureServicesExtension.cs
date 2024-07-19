using System.Globalization;
using Application.Interfaces.Repositories;
using Application.Interfaces.ServiceInterfaces;
using Application.Interfaces.Services;
using Application.Services;
using Application.Services.ReferencedContentProviders;
using Domain.Identity;
using Domain.Models;
using Infrastructure.Data;
using Infrastructure.Mapping.Profiles;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using WebUI.Mapping.Profiles;
using WebUI.ModelBinding.Providers;

namespace WebUI.Extensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        
        var activeConnection = configuration["ActiveConnection"] ?? "DefaultConnection";
        var connectionString = configuration.GetConnectionString(activeConnection) ?? throw new InvalidOperationException($"Connection string '{activeConnection}' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString)
        );
        
        services.AddControllersWithViews(options =>
        {
            options.ModelBinderProviders.Insert(0, new UpdateTrainingPlanModelBinderProvider());
            options.ModelBinderProviders.Insert(0, new FOPModelBindersProvider());
            
            // option.Filters
        })
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization()
            .AddRazorRuntimeCompilation();

        services.AddHttpContextAccessor();

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
            .AddUserManager<SpecializedUserManager>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();


        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
        });

        services.AddApiVersioning(config =>
        {
            config.ApiVersionReader = new UrlSegmentApiVersionReader();
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
        });

        // setup swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => {
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api-docs.xml"));
            
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo{ Title = "Training Tools Web API", Version = "1.0" });
        });
        services.AddVersionedApiExplorer(options => {
            options.GroupNameFormat = "'v'VVV"; //v1
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddHttpClient();
        services.AddAutoMapper(typeof(UiApplicationMappingProfile), typeof(InfrastructureApplicationMappingProfile));

        services.AddApplicationServices(configuration);

        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode;
        }); // required for http logging
        
        return services;
    }
    
    private static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRepository<Group, Guid>, GroupsRepository>();
        services.AddScoped<IGroupsService, GroupsService>();
        services.AddScoped<IFriendsRepository, FriendsRepository>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IFriendsService, FriendsService>();
        services.AddScoped<IExercisesRepository, ExercisesRepository>();
        services.AddScoped<IExercisesService, ExercisesService>();
        services.AddScoped<IExerciseResultsRepository, ExerciseResultsRepository>();
        services.AddScoped<IExerciseResultsService, ExerciseResultsService>();
        services.AddScoped<ITrainingPlansRepository, TrainingPlansRepository>();
        services.AddScoped<ITrainingPlansService, TrainingPlansService>();
        
        services.AddScoped<IReferencedContentProvider, ImagesAndVideosReferencedContentProvider>();
        services.AddTransient<IExerciseResultsToExсelExporter, ExerciseResultsToExcelExporter>();
    }
}