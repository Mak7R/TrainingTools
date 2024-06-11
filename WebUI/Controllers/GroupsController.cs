﻿using Application.Interfaces.ServiceInterfaces;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Models.GroupModels;

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
    public async Task<IActionResult> GetAllGroups()
    {
        var groups = await _groupsService.GetAll();
        var groupViewModels = groups.Select(g => new GroupViewModel { Id = g.Id, Name = g.Name });
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
        var result = await _groupsService.CreateGroup(group);
        
        if (result.IsSuccessful) return RedirectToAction("GetAllGroups");
        
        if (result.ResultObject is AlreadyExistsException exception)
        {
            return this.BadRequestView([exception.Message]);
        }
        else
        {
            return this.ServerErrorView(500, result.Errors);
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
        var result = await _groupsService.UpdateGroup(group);
        
        if (result.IsSuccessful) return RedirectToAction("GetAllGroups");
        if (result.ResultObject is AlreadyExistsException alreadyExistsException)
        {
            return this.BadRequestView([alreadyExistsException.Message]);
        }
        else if (result.ResultObject is NotFoundException notFoundException)
        {
            return this.NotFoundView(notFoundException.Message);
        }
        else
        {
            return this.ServerErrorView(500, result.Errors);
        }
    }

    [HttpGet("delete-group")]
    public async Task<IActionResult> DeleteGroup([FromQuery] Guid groupId)
    {
        var result = await _groupsService.DeleteGroup(groupId);

        if (result.IsSuccessful) return RedirectToAction("GetAllGroups");

        if (result.ResultObject is NotFoundException exception)
        {
            return this.NotFoundView(exception.Message);
        }
        else
        {
            return this.ServerErrorView(500, result.Errors);
        }
    }
}