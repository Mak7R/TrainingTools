using Application.Identity;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace WebUI.Extensions;

public static class InitializeRootAdminExtensions
{
    public static async Task InitializeRootAdminFromConfiguration(this IServiceProvider serviceProvider, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(configuration);
        
        var rootAdmin = configuration.GetSection("RootAdmin").Get<ApplicationUser>();
        if (rootAdmin == null) throw new NullReferenceException("RootAdmin section doesn't exists");

        if (rootAdmin.UserName == null) throw new NullReferenceException("RootAdmin user name is empty");
        if (rootAdmin.Email == null) throw new NullReferenceException("RootAdmin email is empty");
        
        string? password = configuration["RootAdmin:Password"];
        if (password == null) throw new NullReferenceException("RootAdmin password is empty");
        
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var existsRoot = await userManager.FindByIdAsync(rootAdmin.Id.ToString());

        if (existsRoot == null)
        {
            var result = await userManager.CreateAsync(rootAdmin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(rootAdmin, new[] { nameof(Role.User), nameof(Role.Admin), nameof(Role.Root) });
            }
            else
            {
                throw new InvalidOperationException("Error while creating Root Admin.\n" + string.Join('\n', result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            existsRoot.UserName = rootAdmin.UserName;
            existsRoot.Email = rootAdmin.Email;

            await userManager.UpdateAsync(existsRoot);
        }
    }
}