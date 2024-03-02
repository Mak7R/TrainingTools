using Contracts.Services;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.DbContexts;
using SimpleAuthorizer;

var builder = WebApplication.CreateBuilder(args);

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
        
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddDbContext<TrainingToolsDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("TrainingTools"));
});

builder.Services.AddScoped<IUsersAuthorizer, UsersAuthorizer>();
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    // app.UseExceptionHandlingMiddleware();
}
else
{
    app.UseDeveloperExceptionPage();
}

var authSettings = app.Configuration.GetSection("AuthSettings");
AuthMiddleware.SessionIdKey = authSettings["SessionIdKey"] ?? throw new Exception("AuthSetting:SessionIdKey was missed");
AuthMiddleware.HttpContextItemsKey = authSettings["HttpContextItemsKey"] ?? throw new Exception("AuthSetting:HttpContextItemsKey was missed");
app.UseAuthMiddleware();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();

app.Run();