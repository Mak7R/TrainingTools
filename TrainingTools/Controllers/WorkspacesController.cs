using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Index(
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
        var workspaces = await workspacesService.GetAll();
        
        return Json(new WorkspacesViewCollectionBuilder(workspaces).Filter(filter).Order(order).Build());
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
    public async Task<IActionResult> Get([FromRoute] Guid workspaceId,
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();
        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();
        
        var workspace = await workspacesService.Get(w => w.Id == workspaceId);
        if (workspace == null) return NotFound(new ErrorViewModel("Workspace was not found"));

        var groups = (await groupsService.GetAll()).Where(g => g.Workspace.Id == workspace.Id);
        var exercises = (await exercisesService.GetAll()).Where(e => e.Workspace.Id == workspace.Id);
        
        return Json(new FullWorkspaceViewModel(workspace, new GroupsViewCollectionBuilder(groups).Build(), new ExercisesViewCollectionBuilder(exercises).Filter(filter).Order(order).Build()));
    }
    
    [HttpGet("{workspaceId:guid}/info")]
    public async Task<IActionResult> GetInfo([FromRoute] Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
        var workspace = await workspacesService.Get(w => w.Id == workspaceId);
        if (workspace == null) return NotFound(new ErrorViewModel("Workspace was not found"));

        return Json(new WorkspaceViewModel(workspace));
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

        var workspace = await workspacesService.Get(w => w.Id == workspaceId);
        if (workspace == null) return NotFound(new ErrorViewModel("Workspace was not found"));

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

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();

        await workspacesService.Update(workspaceId, w => w.Name = model.Name);
        await authorizedUser.SaveChanges();

        return Ok();
    }
}