using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using Services;
using TrainingTools.Extensions;
using TrainingTools.ViewModels;

namespace TrainingTools.Controllers;

[Route("api/v1/workspaces/{workspaceId:guid}/[controller]")]
public class GroupsController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public GroupsController(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    [HttpGet("")]
    public async Task<IActionResult> GetAll(
        [FromRoute] Guid workspaceId,
        FilterModel filter, 
        OrderModel order)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        var selected = scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>();
        await selected.Select(workspaceId);
        
        
        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();
        var groups = await groupsService.GetAll();
        
        return Json(new GroupsViewCollectionBuilder(groups.Select(g => g.ToGroupViewModel(selected.Permission))).Filter(filter).Order(order).Build());
    }

    [HttpPost("")]
    public async Task<IActionResult> Add([FromRoute] Guid workspaceId, [FromBody] AddGroupModel groupModel)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState.ToModelStateErrorViewModel());
        
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }

        await scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>().Select(workspaceId);
        
        
        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();

        var group = new Group
        {
            Name = groupModel.Name
        };

        await groupsService.Add(group);
        await authorizedUser.SaveChanges();

        return Ok();
    }

    [HttpGet("{groupId:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid groupId, [FromRoute] Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        var selected = scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>();
        await selected.Select(workspaceId);
        

        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();
        var group = await groupsService.Get(g => g.Id == groupId);
        
        return group == null ? NotFound(new ErrorViewModel("Group was not found")) : Json(group.ToGroupViewModel(selected.Permission));
    }
    
    [HttpDelete("{groupId:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid groupId, [FromRoute] Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        await scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>().Select(workspaceId);
        

        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();
        
        await groupsService.Remove(groupId);
        await authorizedUser.SaveChanges();
        
        return Ok();
    }

    [HttpPatch("{groupId:guid}")]
    public async Task<IActionResult> Edit([FromRoute] Guid groupId, [FromBody] EditExerciseModel model, [FromRoute] Guid workspaceId)
    {
        using var scope = _scopeFactory.CreateScope();
        var authorizedUser = scope.ServiceProvider.GetRequiredService<IAuthorizedUser>();
        try { if (!await authorizedUser.Authorize(HttpContext)) return Unauthorized(new ErrorViewModel("User was not authorized")); }
        catch (NotFoundException e) { return NotFound(new ErrorViewModel(e.Message)); }
        
        await scope.ServiceProvider.GetRequiredService<ISelectedWorkspace>().Select(workspaceId);
        

        var groupsService = scope.ServiceProvider.GetRequiredService<IGroupsService>();
        await groupsService.Update(groupId, g => g.Name = model.Name);
        await authorizedUser.SaveChanges();

        return Ok();
    }
}