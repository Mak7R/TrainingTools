using System.ComponentModel.DataAnnotations;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthorizer;
using TrainingTools.Models;

namespace TrainingTools.Controllers;

[Controller]
[Route("[controller]")]
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
        SortBindingModel sorter)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);

        var workspaces = await workspacesService.GetAll();

        var workspacesModels = workspaces.Select(w => new WorkspaceViewModel(w));
        
        if (filter.HasFilters)
        {
            ViewBag.SearchBy = filter.SearchBy!;
            ViewBag.SearchValue = filter.SearchValue!;

            workspacesModels = workspacesModels.Where(filter.SearchBy switch
            {
                nameof(WorkspaceViewModel.Id) => w => w.Id.ToString().Contains(filter.SearchValue!),
                nameof(WorkspaceViewModel.Name) => w => w.Name.Contains(filter.SearchValue!),
                _ => _ => false
            });
        }

        if (sorter.HasSorters)
        {
            ViewBag.SortBy = sorter.SortBy!;
            ViewBag.SortingOption = sorter.SortingOption!;
            
            workspacesModels = (sorter.SortBy, sorter.SortingOption) switch
            {
                (nameof(WorkspaceViewModel.Name), "A-Z") => 
                    workspacesModels.OrderBy(w => w.Name),
                (nameof(WorkspaceViewModel.Name), "Z-A") => 
                    workspacesModels.OrderBy(w => w.Name).Reverse(),
                (nameof(WorkspaceViewModel.Id), "ASCENDING") => 
                    workspacesModels.OrderBy(w => w.Id),
                (nameof(WorkspaceViewModel.Id), "DESCENDING") => 
                    workspacesModels.OrderBy(w => w.Id).Reverse(),
                _ => workspacesModels
            };
        }
        
        return View(workspacesModels);
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult Add()
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");
        
        return View();
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Add([FromForm] AddWorkspaceModel workspaceModel)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);

        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = workspaceModel.Name
        };

        await workspacesService.Add(workspace);
        await usersCollectionService.SaveChanges();
        
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("{workspaceId:guid}")]
    public async Task<IActionResult> Get([FromRoute,Required] Guid workspaceId,
        FilterModel filter, 
        SortBindingModel sorter)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);
        var groupsService = usersCollectionService.GetServiceForUser<IGroupsService>(user);
        var exercisesService = usersCollectionService.GetServiceForUser<IExercisesService>(user);
        
        var workspace = await workspacesService.Get(w => w.Id == workspaceId);
        if (workspace == null) return View("Error", (404, "Workspace was not found"));

        var groups = (await groupsService.GetAll()).Where(g => g.Workspace.Id == workspace.Id);
        var exercises = (await exercisesService.GetAll()).Where(e => e.Workspace.Id == workspace.Id);

        if (filter.HasFilters)
        {
            ViewBag.SearchBy = filter.SearchBy!;
            ViewBag.SearchValue = filter.SearchValue!;

            exercises = exercises.Where(filter.SearchBy switch
            {
                nameof(Exercise.Id) => e => e.Id.ToString().Contains(filter.SearchValue!),
                nameof(Exercise.Name) => e => e.Name.Contains(filter.SearchValue!),
                _ => _ => false
            });
        }

        if (sorter.HasSorters)
        {
            ViewBag.SortBy = sorter.SortBy!;
            ViewBag.SortingOption = sorter.SortingOption!;
            
            exercises = (sorter.SortBy, sorter.SortingOption) switch
            {
                (nameof(Exercise.Name), "A-Z") => 
                    exercises.OrderBy(w => w.Name),
                (nameof(Exercise.Name), "Z-A") => 
                    exercises.OrderBy(w => w.Name).Reverse(),
                (nameof(Exercise.Id), "ASCENDING") => 
                    exercises.OrderBy(w => w.Id),
                (nameof(Exercise.Id), "DESCENDING") => 
                    exercises.OrderBy(w => w.Id).Reverse(),
                _ => exercises
            };
        }
        
        return View(new FullWorkspaceViewModel(workspace, groups, exercises));
    }
    
    [HttpGet]
    [Route("{workspaceId:guid}/info")]
    public async Task<IActionResult> GetInfo([FromRoute][Required] Guid workspaceId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);
            
        var workspace = await workspacesService.Get(w => w.Id == workspaceId);

        if (workspace == null) return View("Error", (404, "Workspace was not found"));

        return View(new WorkspaceViewModel(workspace));
    }
    
    [HttpDelete]
    [Route("{workspaceId:guid}/[action]")]
    public async Task<IActionResult> Delete([FromRoute, Required] Guid workspaceId, [FromBody] DeleteModel model)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return NotFound(new {message = "User was not found"});

        if (user.Password != model.Password)
        {
            ModelState.AddModelError(nameof(DeleteWorkspaceModel.Password), "Wrong password");
            return BadRequest(new {message = "Wrong password"});
        }

        var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);

        var workspace = await workspacesService.Get(w => w.Id == workspaceId);
        if (workspace == null) return NotFound(new { message = "Workspace was not found" });

        try
        {
            await workspacesService.Remove(workspaceId);
            await usersCollectionService.SaveChanges();
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
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);
            
        var workspace = await workspacesService.Get(w => w.Id == workspaceId);

        if (workspace == null) return View("Error", (404, "Workspace was not found"));
        
        return View(new EditWorkspaceModel{Name = workspace.Name});
    }

    [HttpPost]
    [Route("{workspaceId:guid}/[action]")]
    public async Task<IActionResult> Edit([FromRoute, Required] Guid workspaceId, [FromForm, Required] EditWorkspaceModel model)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);

        await workspacesService.Update(workspaceId, w => w.Name = model.Name);
        await usersCollectionService.SaveChanges();

        return RedirectToAction("Index");
    }
}