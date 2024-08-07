﻿using Application.Interfaces.Services;
using AutoMapper;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using WebUI.Models.Group;
using WebUI.Models.Shared;

namespace WebUI.Controllers.Api.v1;

[ApiVersion("1.0")]
public class GroupsController : ApiController
{
    private readonly IGroupsService _groupsService;
    private readonly IMapper _mapper;

    public GroupsController(IGroupsService groupsService, IMapper mapper)
    {
        _groupsService = groupsService;
        _mapper = mapper;
    }

    /// <summary>
    ///     Retrieves a list of all groups, with optional filtering, ordering, and pagination.
    /// </summary>
    /// <param name="filterModel">Supported filters: f_name, f_name-equals</param>
    /// <param name="orderModel">Supported orders: name</param>
    /// <param name="pageModel">Paging params: page, page-size</param>
    /// <returns>List of groups</returns>
    [HttpGet("")]
    [QueryValuesReader<DefaultOrderOptions>]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<GroupViewModel>>> GetAll(FilterViewModel? filterModel,
        OrderViewModel? orderModel, PageViewModel? pageModel)
    {
        var groups = await _groupsService.GetAll(filterModel, orderModel, pageModel);
        return _mapper.Map<List<GroupViewModel>>(groups);
    }

    /// <summary>
    ///     Counts the total number of groups based on optional filter criteria.
    /// </summary>
    /// <param name="filterModel">Supported filters: f_name, f_name-equals</param>
    /// <returns>Total count of groups matching the criteria</returns>
    [HttpGet("count")]
    [QueryValuesReader<DefaultOrderOptions>]
    [AllowAnonymous]
    public async Task<IActionResult> Count(FilterViewModel? filterModel)
    {
        return Ok(await _groupsService.Count(filterModel));
    }

    /// <summary>
    ///     Retrieves a specific group by its ID.
    /// </summary>
    /// <param name="groupId">ID of group</param>
    /// <returns></returns>
    [HttpGet("{groupId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<GroupViewModel>> Get(Guid groupId)
    {
        var group = await _groupsService.GetById(groupId);
        if (group is null)
            return Problem("Group was not found", statusCode: 404);

        return _mapper.Map<GroupViewModel>(group);
    }

    /// <summary>
    ///     Creates new group.
    /// </summary>
    /// <param name="createGroupModel">group model to create</param>
    /// <returns>Created if group was successfully created otherwise returns problem</returns>
    [HttpPost("")]
    [AuthorizeVerifiedRoles(Role.Admin, Role.Root)]
    public async Task<IActionResult> Create(CreateGroupModel createGroupModel)
    {
        var result = await _groupsService.Create(_mapper.Map<Group>(createGroupModel));

        if (result.IsSuccessful)
        {
            if (result.ResultObject is Group group)
                return CreatedAtAction("Get", new { groupId = group.Id });
            return Created();
        }

        if (result.Exception is AlreadyExistsException)
            return Problem("Group already exists in database", statusCode: 400, title: "Group already exists");

        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
            Detail = "An internal server error occurred while processing the request",
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }
        });
    }

    /// <summary>
    ///     Updates group by <see cref="groupId" />
    /// </summary>
    /// <param name="groupId">ID of group</param>
    /// <param name="updateGroupModel">model for updating group</param>
    /// <returns>updated group id update was successful</returns>
    [HttpPut("{groupId:guid}")]
    [AuthorizeVerifiedRoles(Role.Admin, Role.Root)]
    public async Task<IActionResult> Update(Guid groupId, UpdateGroupModel updateGroupModel)
    {
        var group = _mapper.Map<Group>(updateGroupModel);
        group.Id = groupId;
        var result = await _groupsService.Update(group);

        if (result.IsSuccessful)
            return Ok(group);

        if (result.Exception is AlreadyExistsException)
            return Problem("Group already exists", statusCode: 400);

        if (result.Exception is NotFoundException)
            return Problem("Group was not found", statusCode: 404);

        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
            Detail = "An internal server error occurred while processing the request",
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }
        });
    }

    /// <summary>
    ///     Deletes group by <see cref="groupId" />
    /// </summary>
    /// <param name="groupId">ID of group</param>
    /// <returns>no content if successfully deleted otherwise problem</returns>
    [HttpDelete("{groupId:guid}")]
    [AuthorizeVerifiedRoles(Role.Admin, Role.Root)]
    public async Task<IActionResult> Delete(Guid groupId)
    {
        var result = await _groupsService.Delete(groupId);

        if (result.IsSuccessful)
            return NoContent();

        if (result.Exception is NotFoundException)
            return NotFound();

        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
            Detail = "An internal server error occurred while processing the request",
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }
        });
    }
}