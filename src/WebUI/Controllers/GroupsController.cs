using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using AutoMapper;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.ModelBinding.ModelBinders;
using WebUI.Models.Group;
using WebUI.Models.Shared;

namespace WebUI.Controllers;

[Controller]
[AllowAnonymous]
[Route("groups")]
public class GroupsController : Controller
{
    private readonly IGroupsService _groupsService;
    private readonly IMapper _mapper;
    
    public GroupsController(IGroupsService groupsService, IMapper mapper)
    {
        _groupsService = groupsService;
        _mapper = mapper;
    }

    [HttpGet("")]
    [QueryValuesReader<DefaultOrderOptions>]
    public async Task<IActionResult> GetAll(OrderModel? orderModel,[FilterModelBinder] FilterModel? filterModel)
    {
        var groups = await _groupsService.GetAll(orderModel, filterModel);
        
        return View(_mapper.Map<List<GroupViewModel>>(groups));
    }

    [HttpPost("create")]
    [Authorize(Roles = "Root,Admin")]
    [ConfirmUser]
    public async Task<IActionResult> Create([FromForm] CreateGroupModel createGroupModel)
    {
        if (!ModelState.IsValid)
            return this.BadRequestView(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        
        var result = await _groupsService.Create(_mapper.Map<Group>(createGroupModel));
        
        if (result.IsSuccessful) 
            return RedirectToAction("GetAll", "Groups");
        
        if (result.Exception is AlreadyExistsException)
            return this.BadRequestView(result.Errors);
        
        return this.ErrorView(500, result.Errors);
    }

    [HttpPost("update")]
    [Authorize(Roles = "Root,Admin")]
    [ConfirmUser]
    public async Task<IActionResult> Update([FromForm] UpdateGroupModel updateGroupModel)
    {
        if (!ModelState.IsValid)
            return this.BadRequestView(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        
        var result = await _groupsService.Update(_mapper.Map<Group>(updateGroupModel));
        
        if (result.IsSuccessful) 
            return RedirectToAction("GetAll", "Groups");
        
        if (result.Exception is AlreadyExistsException)
            return this.BadRequestView(result.Errors);
        
        if (result.Exception is NotFoundException)
            return this.NotFoundView(result.Errors);
        
        return this.ErrorView(500, result.Errors);
    }

    [HttpGet("delete")]
    [Authorize(Roles = "Root,Admin")]
    [ConfirmUser]
    public async Task<IActionResult> Delete([FromQuery] Guid groupId)
    {
        var result = await _groupsService.Delete(groupId);

        if (result.IsSuccessful) 
            return RedirectToAction("GetAll");

        if (result.Exception is NotFoundException)
            return this.NotFoundView(result.Errors);
        
        return this.ErrorView(500, result.Errors);
    }
}