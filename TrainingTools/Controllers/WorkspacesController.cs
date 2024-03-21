using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Services;
using TrainingTools.Extensions;
using TrainingTools.ViewModels;

namespace TrainingTools.Controllers;

[Controller]
[Route("api/v1/[controller]")]
public class WorkspacesController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public WorkspacesController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll(
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
        var workspaces = await workspacesService.GetRange(w => w.OwnerId == authorizedUser.User.Id);
        
        return Json(new WorkspacesViewCollectionBuilder(workspaces.Select(w => w.ToWorkspaceViewModel(WorkspacePermission.OwnerPermission))).Filter(filter).Order(order).Build());
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddWorkspaceModel workspaceModel)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();

        var workspace = new Workspace
        {
            Name = workspaceModel.Name
        };

        await workspacesService.Add(workspace);
        await authorizedUser.SaveChanges();

        return Ok();
    }

    [HttpGet("{workspaceId:guid}")]
    public async Task<IActionResult> GetFull([FromRoute] Guid workspaceId,
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();
        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();
        
        var selected = scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>();
        await selected.Select(workspaceId);

        var groups = await groupsService.GetAll();
        var exercises = await exercisesService.GetAll();

        var model = selected.Workspace.ToFullWorkspaceViewModel(groups, exercises, selected.Permission);
        new ExercisesViewCollectionBuilder(model.Exercises).Filter(filter).Order(order);
        return Json(model);
    }
    
    [HttpGet("{workspaceId:guid}/info")]
    public async Task<IActionResult> GetInfo([FromRoute] Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        var selected = scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>();
        await selected.Select(workspaceId);

        return Json(selected.Workspace.ToWorkspaceViewModel(selected.Permission));
    }
    
    [HttpDelete("{workspaceId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid workspaceId, [FromBody] DeleteModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        if (!authorizedUser.ConfirmPassword(model.Password))
        {
            ModelState.AddModelError(nameof(DeleteModel.Password), "Wrong password");
            return BadRequest(ModelState.ToModelStateErrorViewModel());
        }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();

        await workspacesService.Remove(workspaceId);
        await authorizedUser.SaveChanges();
        
        return Ok();
    }

    [HttpPatch("{workspaceId:guid}")]
    public async Task<IActionResult> Edit([FromRoute] Guid workspaceId, [FromBody] EditWorkspaceModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        await scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>().Select(workspaceId);
        
        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
        
        await workspacesService.Update(w =>
        {
            w.Name = model.Name;
            if (authorizedUser.User.Id == w.OwnerId)
            {
                w.IsPublic = model.IsPublic;
            }
        });
        await authorizedUser.SaveChanges();

        return Ok();
    }
}