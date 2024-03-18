using Contracts.Client.Services;
using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.Client;
using Services.DbContexts;
using SimpleAuthorizer;
using TrainingTools.Components;
using TrainingTools.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ISessionContainer<Guid, Guid>, AutoClearedSessionContainer>((_) =>
{
    var sessionContainer = new AutoClearedSessionContainer();
            
    var checkTimeConfig = builder.Configuration.GetSection("SessionAutoClearingFrequency:ToCheck");
    (int hours, int minutes, int seconds) checkTime = 
        (int.Parse(checkTimeConfig["hours"]!), int.Parse(checkTimeConfig["minutes"]!), int.Parse(checkTimeConfig["seconds"]!));

    var sessionTimeConfig = builder.Configuration.GetSection("SessionAutoClearingFrequency:ToClear");
    (int hours, int minutes, int seconds) sessionTime = 
        (int.Parse(sessionTimeConfig["hours"]!), int.Parse(sessionTimeConfig["minutes"]!), int.Parse(sessionTimeConfig["seconds"]!)); 
            
    sessionContainer.CleaningChecksFrequency = new TimeSpan(checkTime.hours, checkTime.minutes, checkTime.seconds);
    sessionContainer.SessionTime = new TimeSpan(sessionTime.hours, sessionTime.minutes, sessionTime.seconds);
    return sessionContainer;
});
        

builder.Services.AddDbContext<TrainingToolsDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("TrainingTools"));
});

// client-side
builder.Services.AddHttpClient();
builder.Services.AddScoped<IFetchService, DefaultFetchService>();
builder.Services.AddScoped<ICookiesProvider, BrowserCookiesProvider>();
builder.Services.AddScoped<RequestBuilder>();
builder.Services.AddSingleton<ILinkGenerator, ConfigLinkGenerator>();
builder.Services.AddTransient<Linker>();

// both-side
builder.Services.AddHttpContextAccessor();

// server-side
builder.Services.AddScoped<ICookiesSession, CookiesSession>();
builder.Services.AddScoped<IAuthorizedUser, AuthorizedUser>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IWorkspacesService, WorkspacesService>();
builder.Services.AddScoped<IExercisesService, ExercisesService>();
builder.Services.AddScoped<IGroupsService, GroupsService>();
builder.Services.AddScoped<IExerciseResultsService, ExerciseResultsService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TrainingToolsDbContext>().Database;
    db.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // for dev environment
}
app.UseExceptionHandlingMiddleware();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();