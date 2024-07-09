using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Mappers;
using WebUI.ModelBinding.CustomModelBinders;
using WebUI.Models.GroupModels;
using WebUI.Models.SharedModels;

namespace WebUI.Controllers;

[Controller]
[Authorize(Roles = "Admin,Root")]
[Route("groups")]
public class GroupsController : Controller
{
    private readonly IGroupsService _groupsService;

    // ReSharper disable once ConvertToPrimaryConstructor
    public GroupsController(IGroupsService groupsService)
    {
        _groupsService = groupsService;
    }

    [HttpGet("")]
    [TypeFilter(typeof(QueryValuesProvidingActionFilter), Arguments = new object[] { typeof(DefaultOrderOptions) })]
    public async Task<IActionResult> GetAllGroups(OrderModel? orderModel,[ModelBinder(typeof(FilterModelBinder))] FilterModel? filterModel)
    {
        var groups = await _groupsService.GetAll(orderModel, filterModel);
        var groupViewModels = groups.Select(g => g.ToGroupViewModel());
        return View(groupViewModels);
    }

    [HttpPost("add-group")]
    public async Task<IActionResult> AddGroup([FromForm] AddGroupModel addGroupModel)
    {
        if (!ModelState.IsValid)
        {
            return this.BadRequestView(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        }
        
        var group = new Group{Name = addGroupModel.Name};
        var result = await _groupsService.Create(group);
        
        if (result.IsSuccessful) return RedirectToAction("GetAllGroups");
        
        if (result.Exception is AlreadyExistsException)
        {
            return this.BadRequestView(result.Errors);
        }
        else
        {
            return this.ErrorView(500, result.Errors);
        }
    }

    [HttpPost("edit-group")]
    public async Task<IActionResult> EditGroup([FromForm] EditGroupModel editGroupModel)
    {
        if (!ModelState.IsValid)
        {
            return this.BadRequestView(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        }
        var group = new Group{Id = editGroupModel.Id, Name = editGroupModel.Name};
        var result = await _groupsService.Update(group);
        
        if (result.IsSuccessful) return RedirectToAction("GetAllGroups", "Groups");
        if (result.Exception is AlreadyExistsException)
        {
            return this.BadRequestView(result.Errors);
        }
        else if (result.Exception is NotFoundException)
        {
            return this.NotFoundView(result.Errors);
        }
        else
        {
            return this.ErrorView(500, result.Errors);
        }
    }

    [HttpGet("delete-group")]
    public async Task<IActionResult> DeleteGroup([FromQuery] Guid groupId)
    {
        var result = await _groupsService.Delete(groupId);

        if (result.IsSuccessful) return RedirectToAction("GetAllGroups");

        if (result.Exception is NotFoundException)
        {
            return this.NotFoundView(result.Errors);
        }
        else
        {
            return this.ErrorView(500, result.Errors);
        }
    }
}