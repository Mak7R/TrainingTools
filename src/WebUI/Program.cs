using System.Globalization;
using Domain.Enums;
using Domain.Identity;
using Infrastructure.Data;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Mapping.Profiles;
using WebUI.Middlewares;
using WebUI.ModelBinding.Providers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services);
    });

var activeConnection = builder.Configuration["ActiveConnection"] ?? "DefaultConnection";

var connectionString = builder.Configuration.GetConnectionString(activeConnection) ?? throw new InvalidOperationException($"Connection string '{activeConnection}' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
);

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new FilterModelBinderProvider());
    options.ModelBinderProviders.Insert(0, new UpdateTrainingPlanModelBinderProvider());
    
    // option.Filters
})
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization()
    .AddRazorRuntimeCompilation();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<RequestLocalizationOptions>(options =>
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

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequiredLength = 4;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = false;

        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.!#$%^&*()_-+=";
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});

builder.Services.AddApiVersioning(config =>
{
    config.ApiVersionReader = new UrlSegmentApiVersionReader();
    config.DefaultApiVersion = new ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;
});

// setup swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api-docs.xml"));
    
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo{ Title = "Training Tools Web API", Version = "1.0" });
});
builder.Services.AddVersionedApiExplorer(options => {
    options.GroupNameFormat = "'v'VVV"; //v1
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddAutoMapper(typeof(UiApplicationMappingProfile));

builder.Services.AddDefaultServices();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode;
}); // required for http logging



var app = builder.Build();

using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    await scope.ServiceProvider.InitializeDataBase<ApplicationDbContext>(Enum.GetNames<Role>());
    await scope.ServiceProvider.InitializeRootAdminFromConfiguration(builder.Configuration);
}

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseExceptionHandlingMiddleware();

Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot");

app.UseSerilogRequestLogging();
app.UseHttpLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
