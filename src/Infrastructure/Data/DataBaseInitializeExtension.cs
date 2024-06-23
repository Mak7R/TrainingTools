using Domain.Identity;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data;

public static class DataBaseInitializeExtension
{
    public static void Migrate(this DbContext dbContext)
    {
        if (dbContext.Database.IsRelational()) dbContext.Database.Migrate();
    }

    public static async Task InitializeRoles(this IServiceProvider serviceProvider, IEnumerable<string> roles)
    {
        var rolesManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>(); 
        foreach (var roleName in roles)
        {
            if (await rolesManager.FindByNameAsync(roleName) is not null) continue;
            await rolesManager.CreateAsync(new ApplicationRole{ Name = roleName });
        }
    }

    public static async Task InitializeGroups(this ApplicationDbContext dbContext)
    {
        if (!dbContext.Groups.Any())
        {
            await dbContext.Groups.AddAsync(new GroupEntity { Id = Guid.NewGuid(), Name = "Грудь" });
            await dbContext.Groups.AddAsync(new GroupEntity { Id = Guid.NewGuid(), Name = "Спина" });
            await dbContext.Groups.AddAsync(new GroupEntity { Id = Guid.NewGuid(), Name = "Ноги" });
            await dbContext.Groups.AddAsync(new GroupEntity { Id = Guid.NewGuid(), Name = "Плечи" });
            await dbContext.Groups.AddAsync(new GroupEntity { Id = Guid.NewGuid(), Name = "Бицепс" });
            await dbContext.Groups.AddAsync(new GroupEntity { Id = Guid.NewGuid(), Name = "Трицепс" });

            await dbContext.SaveChangesAsync();
        }
    }
    
    public static async Task InitializeDataBase<T>(this IServiceProvider serviceProvider, IEnumerable<string> roles) where T: DbContext
    {
        var dbContext = serviceProvider.GetRequiredService<T>();
        dbContext.Migrate();

        await serviceProvider.InitializeRoles(roles);
        if (dbContext is ApplicationDbContext appDbContext) await appDbContext.InitializeGroups();
    }
}