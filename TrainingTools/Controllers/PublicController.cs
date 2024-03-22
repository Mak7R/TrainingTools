using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Extensions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Services;
using TrainingTools.ViewModels;

namespace TrainingTools.Controllers;

[Route("api/v1/public")]
public class PublicController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PublicController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    [Route("users")]
    public async Task<IActionResult> GetAllUsers(
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();

        var users = await usersService.GetAll();
        
        return Json(new PublicUserViewCollectionBuilder(users.Select(u => u.ToPublicUserViewModel())).Filter(filter).Order(order).Build());
    }
    
    [Route("workspaces")]
    public async Task<IActionResult> GetAllWorkspaces(
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
        
        try
        {
            if (await authorizedUser.Authorize(HttpContext))
            {
                var followersService = scope.ServiceProvider.GetRequiredService<IFollowersService>();
                var followsWorkspaces = (await followersService.GetFollows()).Select(fr => (fr.WorkspaceId, fr.FollowerRights));
                var notOwnWorkspaces = await workspacesService.GetRange(w => w.IsPublic && w.OwnerId != authorizedUser.User.Id);

                return Json(new PublicWorkspacesViewCollectionBuilder(notOwnWorkspaces.Select(w => w.ToPublicWorkspaceViewModel(GetPermission(w)))).Filter(filter).Order(order).Build());

                WorkspacePermission GetPermission(Workspace workspace)
                {
                    foreach (var follow in followsWorkspaces)
                    {
                        if (follow.WorkspaceId == workspace.Id)
                        {
                            return follow.FollowerRights switch
                            {
                                FollowerRights.PendingAccess => WorkspacePermission.FollowedPermission,
                                FollowerRights.ViewOnly => WorkspacePermission.ViewPermission,
                                FollowerRights.UseOnly => WorkspacePermission.UsePermission,
                                FollowerRights.All => WorkspacePermission.EditPermission,
                                _ => throw new ArgumentOutOfRangeException()
                            };
                        }
                    }

                    return WorkspacePermission.PermissionDenied;
                }
            }
        }
        catch (NotFoundException)
        {
            // ignored
        }
        
        var workspaces = await workspacesService.GetRange(w => w.IsPublic);
        
        return Json(new PublicWorkspacesViewCollectionBuilder(workspaces.Select(w => w.ToPublicWorkspaceViewModel(WorkspacePermission.Unauthorized))).Filter(filter).Order(order).Build());
    }
    
    [Route("users/{userId:guid}")]
    public async Task<IActionResult> GetPublicUser(Guid userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var usersService = scope.ServiceProvider.GetRequiredService<IUsersService>();

        var user = await usersService.Get(u => u.Id == userId);

        return Json(user.ToPublicUserViewModel());
    }
    
    [Route("workspaces/{workspaceId:guid}")]
    public async Task<IActionResult> GetPublicWorkspace(Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        
        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
        var workspace = await workspacesService.Get(workspaceId);
        if (workspace == null) return NotFound(new ErrorViewModel("Workspace was not found"));
        
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try
        {
            if (await authorizedUser.Authorize(HttpContext))
            {
                if (workspace.OwnerId == authorizedUser.User.Id)
                {
                    return StatusCode(StatusCodes.Status302Found);
                }

                var selectedWorkspace = scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>();
                await selectedWorkspace.Select(workspaceId);
                
                var follower = selectedWorkspace.Workspace.Followers.FirstOrDefault(fr => fr.FollowerId == authorizedUser.User.Id);
                if (follower != null && follower.FollowerRights.HasViewRights())
                {
                    return StatusCode(StatusCodes.Status302Found);
                }

                if (follower != null)
                {
                    return Json(workspace.ToPublicWorkspaceViewModel(WorkspacePermission.FollowedPermission));
                }

                return Json(workspace.ToPublicWorkspaceViewModel(WorkspacePermission.PermissionDenied));
            }
        }
        catch (NotFoundException)
        {
            // ignored
        }

        return Json(workspace.ToPublicWorkspaceViewModel(WorkspacePermission.Unauthorized));
    }
}