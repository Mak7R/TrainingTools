using System.ComponentModel.DataAnnotations;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthorizer;
using TrainingTools.Models;

namespace TrainingTools.Controllers;

[Controller]
[Route("workspaces")]
public class WorkspaceController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public WorkspaceController(IServiceScopeFactory scopeFactory)
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
        if (userId == null) return RedirectToAction("Login", "User");

        IEnumerable<Workspace> workspaces;
        using (var scope = _scopeFactory.CreateScope())
        {
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.GetUserAsync(u => u.Id == userId);
            
            if (user == null) return View("Error", (404, "User was not found"));

            var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);

            workspaces = await workspacesService.GetWorkspacesAsync();
        }
        
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
        if (userId == null) return RedirectToAction("Login", "User");
        
        return View();
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Add([FromForm] AddWorkspaceModel workspaceModel)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "User");

        using (var scope = _scopeFactory.CreateScope())
        {
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.GetUserAsync(u => u.Id == userId);
            
            if (user == null) return View("Error", (404, "User was not found"));

            var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);

            var workspace = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = workspaceModel.Name
            };

            await workspacesService.AddAsync(workspace);
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("{workspaceId:guid}")]
    public async Task<IActionResult> Get([FromRoute][Required] Guid workspaceId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "User");

        Workspace? workspace;
        using (var scope = _scopeFactory.CreateScope())
        {
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.GetUserAsync(u => u.Id == userId);
            
            if (user == null) return View("Error", (404, "User was not found"));

            var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);
            
            workspace = await workspacesService.GetWorkspaceAsync(w => w.Id == workspaceId);
        }

        if (workspace == null) return View("Error", (404, "Workspace was not found"));

        return View(new WorkspaceViewModel(workspace));
    }
    
    [HttpGet]
    [Route("{workspaceId:guid}/[action]")]
    public async Task<IActionResult> Delete([FromRoute][Required] Guid workspaceId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "User");

        Workspace? workspace;
        using (var scope = _scopeFactory.CreateScope())
        {
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.GetUserAsync(u => u.Id == userId);
            
            if (user == null) return View("Error", (404, "User was not found"));

            var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);
            
            workspace = await workspacesService.GetWorkspaceAsync(w => w.Id == workspaceId);
        }

        if (workspace == null) return View("Error", (404, "Workspace was not found"));
        
        return View(new DeleteWorkspaceModel{WorkspaceName = workspace.Name});
    }
    
    [HttpPost]
    [Route("{workspaceId:guid}/[action]")]
    public async Task<IActionResult> Delete([FromRoute][Required] Guid workspaceId, [FromForm] DeleteWorkspaceModel model)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "User");
        
        using (var scope = _scopeFactory.CreateScope())
        {
            var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
            var user = await usersCollectionService.GetUserAsync(u => u.Id == userId);
            
            if (user == null) return View("Error", (404, "User was not found"));

            if (user.Password != model.Password)
            {
                ModelState.AddModelError(nameof(DeleteWorkspaceModel.Password), "Wrong password");
                return View(model);
            }

            var workspacesService = usersCollectionService.GetServiceForUser<IWorkspacesService>(user);

            var workspace = await workspacesService.GetWorkspaceAsync(w => w.Id == workspaceId);
            if (workspace == null) return View("Error", (404, "Workspace was not found"));
            
            await workspacesService.RemoveAsync(workspace);
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult Edit([FromQuery][Required] Guid workspaceId)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("[action]")]
    public IActionResult Edit(int model)
    {
        throw new NotImplementedException();
    }
}