using System.ComponentModel.DataAnnotations;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using TrainingTools.Models;
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
    [Route("")]
    public async Task<IActionResult> Index(
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorModel(e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
        var workspaces = await workspacesService.GetAll();
        
        ViewBag.FilterBy = filter.FilterBy;
        ViewBag.FilterValue = filter.FilterValue;
        ViewBag.OrderBy = order.OrderBy;
        ViewBag.OrderOption = order.OrderOption;
        
        return Json(new WorkspacesViewCollectionBuilder(workspaces).Filter(filter).Order(order).Build());
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> Add()
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users"); }
        catch (NotFoundException e) { return View("Error", (404, e.Message)); }
        
        return View();
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Add([FromForm] AddWorkspaceModel workspaceModel)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users"); }
        catch (NotFoundException e) { return View("Error", (404, e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();

        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = workspaceModel.Name
        };

        await workspacesService.Add(workspace);
        await authorizedUser.SaveChanges();
        
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("{workspaceId:guid}")]
    public async Task<IActionResult> Get([FromRoute,Required] Guid workspaceId,
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users"); }
        catch (NotFoundException e) { return View("Error", (404, e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();
        var exercisesService = scope.ServiceProvider.GetRequiredService<IExercisesService>();
        
        var workspace = await workspacesService.Get(w => w.Id == workspaceId);
        if (workspace == null) return View("Error", (404, "Workspace was not found"));

        var groups = (await groupsService.GetAll()).Where(g => g.Workspace.Id == workspace.Id);
        var exercises = (await exercisesService.GetAll()).Where(e => e.Workspace.Id == workspace.Id);
        
        ViewBag.FilterBy = filter.FilterBy;
        ViewBag.FilterValue = filter.FilterValue;
        ViewBag.OrderBy = order.OrderBy;
        ViewBag.OrderOption = order.OrderOption;
        
        return View(new FullWorkspaceViewModel(workspace, new GroupsViewCollectionBuilder(groups).Build(), new ExercisesViewCollectionBuilder(exercises).Filter(filter).Order(order).Build()));
    }
    
    [HttpGet]
    [Route("{workspaceId:guid}/info")]
    public async Task<IActionResult> GetInfo([FromRoute][Required] Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users"); }
        catch (NotFoundException e) { return View("Error", (404, e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
        var workspace = await workspacesService.Get(w => w.Id == workspaceId);
        if (workspace == null) return View("Error", (404, "Workspace was not found"));

        return View(new WorkspaceViewModel(workspace));
    }
    
    [HttpDelete]
    [Route("{workspaceId:guid}/[action]")]
    public async Task<IActionResult> Delete([FromRoute, Required] Guid workspaceId, [FromBody] DeleteModel model)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users"); }
        catch (NotFoundException e) { return View("Error", (404, e.Message)); }
        
        if (!authorizedUser.ConfirmPassword(model.Password))
        {
            ModelState.AddModelError(nameof(DeleteWorkspaceModel.Password), "Wrong password");
            return BadRequest(new {message = "Wrong password"});
        }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();

        var workspace = await workspacesService.Get(w => w.Id == workspaceId);
        if (workspace == null) return NotFound(new { message = "Workspace was not found" });

        try
        {
            await workspacesService.Remove(workspaceId);
            await authorizedUser.SaveChanges();
        }
        catch (Exception e)
        {
            return StatusCode(500, new {message = e.Message});
        }
        
        return Ok();
    }

    [HttpGet]
    [Route("{workspaceId:guid}/[action]")]
    public async Task<IActionResult> Edit([FromRoute][Required] Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users"); }
        catch (NotFoundException e) { return View("Error", (404, e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();
            
        var workspace = await workspacesService.Get(w => w.Id == workspaceId);

        if (workspace == null) return View("Error", (404, "Workspace was not found"));
        
        return View(new EditWorkspaceModel{Name = workspace.Name});
    }

    [HttpPost]
    [Route("{workspaceId:guid}/[action]")]
    public async Task<IActionResult> Edit([FromRoute, Required] Guid workspaceId, [FromForm, Required] EditWorkspaceModel model)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return RedirectToAction("Login", "Users"); }
        catch (NotFoundException e) { return View("Error", (404, e.Message)); }

        var workspacesService = scope.ServiceProvider.GetRequiredService<IWorkspacesService>();

        await workspacesService.Update(workspaceId, w => w.Name = model.Name);
        await authorizedUser.SaveChanges();

        return RedirectToAction("Index");
    }
}