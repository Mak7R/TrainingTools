using System.ComponentModel.DataAnnotations;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using SimpleAuthorizer;
using TrainingTools.Models;

namespace TrainingTools.Controllers;

[Route("workspaces/[controller]/{groupId:guid}")]
public class GroupsController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public GroupsController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    [HttpGet]
    [Route("/workspaces/{workspaceId:guid}/[controller]")]
    public async Task<IActionResult> Index(
        [Required, FromRoute] Guid workspaceId,
        FilterModel filter, 
        OrderModel order)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var groupsService = usersCollectionService.GetServiceForUser<IGroupsService>(user);

        var groups = (await groupsService.GetAll()).Where(g => g.Workspace.Id == workspaceId);

        ViewBag.FilterBy = filter.FilterBy;
        ViewBag.FilterValue = filter.FilterValue;
        ViewBag.OrderBy = order.OrderBy;
        ViewBag.OrderOption = order.OrderOption;
        
        return View(new GroupsViewCollectionBuilder(groups).Filter(filter).Order(order).Build());
    }

    [HttpGet]
    [Route("/workspaces/{workspaceId:guid}/[controller]/[action]")]
    public IActionResult Add()
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");
        
        return View();
    }

    [HttpPost]
    [Route("/workspaces/{workspaceId:guid}/[controller]/[action]")]
    public async Task<IActionResult> Add([Required, FromRoute] Guid workspaceId, [FromForm] AddGroupModel groupModel)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var groupsService = usersCollectionService.GetServiceForUser<IGroupsService>(user);

        var group = new Group()
        {
            Id = Guid.NewGuid(),
            Name = groupModel.Name,
            WorkspaceId = workspaceId
        };

        await groupsService.Add(group);
        await usersCollectionService.SaveChanges();

        return RedirectToAction("Index", new {workspaceId});
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Get([FromRoute, Required] Guid groupId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
            
        if (user == null) return View("Error", (404, "User was not found"));

        var groupsService = usersCollectionService.GetServiceForUser<IGroupsService>(user);
            
        var group = await groupsService.Get(g => g.Id == groupId);

        if (group == null) return View("Error", (404, "Group was not found"));
        
        return View(new GroupViewModel(group));
    }
    
    [HttpGet, HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Delete([FromRoute, Required] Guid groupId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
        if (user == null) return View("Error", (404, "User was not found"));

        var groupsService = usersCollectionService.GetServiceForUser<IGroupsService>(user);
        var group = await groupsService.Get(g => g.Id == groupId);
        try
        {
            await groupsService.Remove(groupId);
            await usersCollectionService.SaveChanges();
        }
        catch (Exception e)
        {
            return View("Error", (500, e.Message));
        }
        
        return RedirectToAction("Index", new {workspaceId = group!.Workspace.Id});
    }

    [HttpGet]
    [Route("[action]")]
    public async Task<IActionResult> Edit([FromRoute, Required] Guid groupId)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
        if (user == null) return View("Error", (404, "User was not found"));

        var groupsService = usersCollectionService.GetServiceForUser<IGroupsService>(user);
        var group = await groupsService.Get(e => e.Id == groupId);
        if (group == null) return View("Error", (404, "Group was not found"));
        
        return View(new EditGroupModel{Name = group.Name});
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Edit([FromRoute, Required] Guid groupId, [FromForm, Required] EditExerciseModel model)
    {
        var userId = HttpContext.GetIdFromSession();
        if (userId == null) return RedirectToAction("Login", "Users");

        using var scope = _scopeFactory.CreateScope();
        var usersCollectionService = scope.ServiceProvider.GetRequiredService<IUsersAuthorizer>();
        var user = await usersCollectionService.Get(u => u.Id == userId);
        if (user == null) return View("Error", (404, "User was not found"));

        var groupsService = usersCollectionService.GetServiceForUser<IGroupsService>(user);
        await groupsService.Update(groupId, g => g.Name = model.Name);
        await usersCollectionService.SaveChanges();
        
        var group = await groupsService.Get(g => g.Id == groupId);
        if (group == null) return View("Error", (404, "Group was not found"));

        return RedirectToAction("Index", new {workspaceId = group.Workspace.Id});
    }
}