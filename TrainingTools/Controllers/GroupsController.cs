using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using TrainingTools.Extensions;
using TrainingTools.Models;
using TrainingTools.ViewModels;

namespace TrainingTools.Controllers;

[Route("api/v1/workspaces")]
public class GroupsController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public GroupsController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    [HttpGet("{workspaceId:guid}/[controller]")]
    public async Task<IActionResult> Index(
        [FromRoute] Guid workspaceId,
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();

        var groups = (await groupsService.GetAll()).Where(g => g.Workspace.Id == workspaceId);
        
        return Json(new GroupsViewCollectionBuilder(groups).Filter(filter).Order(order).Build());
    }

    [HttpPost("{workspaceId:guid}/[controller]")]
    public async Task<IActionResult> Add([FromRoute] Guid workspaceId, [FromBody] AddGroupModel groupModel)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();

        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = groupModel.Name,
            WorkspaceId = workspaceId
        };

        await groupsService.Add(group);
        await authorizedUser.SaveChanges();

        return Ok();
    }

    [HttpGet("[controller]/{groupId:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid groupId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();
        var group = await groupsService.Get(g => g.Id == groupId);
        
        return group == null ? NotFound(new ErrorViewModel("Group was not found")) : Json(new GroupViewModel(group));
    }
    
    [HttpDelete("[controller]/{groupId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid groupId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();
        
        await groupsService.Remove(groupId);
        await authorizedUser.SaveChanges();
        
        return Ok();
    }

    [HttpPatch("[controller]/{groupId:guid}")]
    public async Task<IActionResult> Edit([FromRoute] Guid groupId, [FromBody] EditExerciseModel model)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();
        await groupsService.Update(groupId, g => g.Name = model.Name);
        await authorizedUser.SaveChanges();

        return Ok();
    }
}