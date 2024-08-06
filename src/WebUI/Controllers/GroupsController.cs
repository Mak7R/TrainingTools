using Application.Interfaces.Services;
using Application.Models.Shared;
using AutoMapper;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Models.Group;
using WebUI.Models.Shared;

namespace WebUI.Controllers;

[Controller]
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
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        FilterViewModel? filterModel,
        OrderViewModel? orderModel,
        PageViewModel? pageModel)
    {
        pageModel ??= new PageViewModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            var defaultPageSize = 10;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        }

        ViewBag.GroupsCount = await _groupsService.Count(filterModel);

        var groups = await _groupsService.GetAll(filterModel, orderModel, pageModel);

        return View(_mapper.Map<List<GroupViewModel>>(groups));
    }

    [HttpPost("create")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    public async Task<IActionResult> Create([FromForm] CreateGroupModel createGroupModel)
    {
        if (!ModelState.IsValid)
            return this.BadRequestRedirect(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        var result = await _groupsService.Create(_mapper.Map<Group>(createGroupModel));

        if (result.IsSuccessful)
            return RedirectToAction("GetAll", "Groups");

        if (result.Exception is AlreadyExistsException)
            return this.BadRequestRedirect(result.Errors);

        return this.ErrorRedirect(500, result.Errors);
    }

    [HttpPost("update")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    public async Task<IActionResult> Update([FromForm] UpdateGroupModel updateGroupModel)
    {
        if (!ModelState.IsValid)
            return this.BadRequestRedirect(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

        var result = await _groupsService.Update(_mapper.Map<Group>(updateGroupModel));

        if (result.IsSuccessful)
            return RedirectToAction("GetAll", "Groups");

        if (result.Exception is AlreadyExistsException)
            return this.BadRequestRedirect(result.Errors);

        if (result.Exception is NotFoundException)
            return this.NotFoundRedirect(result.Errors);

        return this.ErrorRedirect(500, result.Errors);
    }

    [HttpGet("delete")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    public async Task<IActionResult> Delete([FromQuery] Guid groupId)
    {
        var result = await _groupsService.Delete(groupId);

        if (result.IsSuccessful)
            return RedirectToAction("GetAll");

        if (result.Exception is NotFoundException)
            return this.NotFoundRedirect(result.Errors);

        return this.ErrorRedirect(500, result.Errors);
    }
}