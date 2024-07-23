using Domain.Enums;
using Infrastructure.Data;
using Microsoft.Extensions.Options;
using Serilog;
using WebUI.Extensions;
using WebUI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services);
    });

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    try
    {
        await scope.ServiceProvider.InitializeDataBase<ApplicationDbContext>(Enum.GetNames<Role>());
        await scope.ServiceProvider.InitializeRootAdminFromConfiguration(builder.Configuration);
    }
    catch (Exception e)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();
        logger.LogCritical(e, "Error when db initialize");
    }
}

app.UseHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseExceptionHandlingMiddleware();

app.UseSerilogRequestLogging();
app.UseHttpLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwagger(options => {
    options.RouteTemplate = "api/{documentName}/index.json";
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/api/v1/index.json", "API V1");
    options.RoutePrefix = "api-docs";
});

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
