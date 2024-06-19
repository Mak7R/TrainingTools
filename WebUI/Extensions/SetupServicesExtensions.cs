using Application.Interfaces.RepositoryInterfaces;
using Application.Interfaces.ServiceInterfaces;
using Application.Services;
using Infrastructure.Repositories;

namespace WebUI.Extensions;

public static class SetupServicesExtensions
{
    public static void AddDefaultServices(this IServiceCollection services)
    {
        services.AddScoped<IGroupsRepository, GroupsRepository>();
        services.AddScoped<IGroupsService, GroupsService>();
        services.AddScoped<IFriendsRepository, FriendsRepository>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IFriendsService, FriendsService>();
        services.AddScoped<IExercisesRepository, ExercisesRepository>();
        services.AddScoped<IExercisesService, ExercisesService>();
        services.AddScoped<IExerciseResultsRepository, ExerciseResultsRepository>();
        services.AddScoped<IExerciseResultsService, ExerciseResultsService>();
        
        services.AddTransient<IExerciseResultsToExelExporter, ExerciseResultsToExcelExporter>();
    }
}