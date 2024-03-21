using Contracts.Exceptions;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Services;
using TrainingTools.Extensions;
using TrainingTools.ViewModels;

namespace TrainingTools.Controllers;

[Route("api/v1/workspaces/{workspaceId:guid}/followers")]
public class FollowersController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FollowersController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    [HttpGet("")]
    public async Task<IActionResult> GetAll(Guid workspaceId,
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        await scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>().Select(workspaceId);
        
        var workspacesService = scope.ServiceProvider.GetRequiredService<IFollowersService>();
        var followers = await workspacesService.GetFollowers();
        return Json(new FollowersViewCollectionBuilder(followers.Select(f => f.ToFollowerViewModel())).Filter(filter).Order(order).Build());
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add(Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        var followersService = scope.ServiceProvider.GetRequiredService<IFollowersService>();
        await followersService.AddFollower(workspaceId);
        await authorizedUser.SaveChanges();
        
        return Ok();
    }

    [HttpPatch("{followerId:guid}")]
    public async Task<IActionResult> Edit(Guid workspaceId, Guid followerId, [FromBody] EditFollowerModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        await scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>().Select(workspaceId);
        
        var followersService = scope.ServiceProvider.GetRequiredService<IFollowersService>();
        await followersService.UpdateFollower(followerId, fr => fr.FollowerRights = model.Rights);
        await authorizedUser.SaveChanges();
        
        return Ok();
    }

    [HttpDelete("{followerId:guid}")]
    public async Task<IActionResult> Delete(Guid workspaceId, Guid followerId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        await scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>().Select(workspaceId);
        
        var followersService = scope.ServiceProvider.GetRequiredService<IFollowersService>();
        await followersService.RemoveFollower(followerId);
        await authorizedUser.SaveChanges();
        
        return Ok();
    }
}