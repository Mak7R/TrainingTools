using Application.Interfaces.ServiceInterfaces;
using Application.Interfaces.Services;
using Application.Models.Shared;
using AutoMapper;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using WebUI.ModelBinding.ModelBinders;
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
    /// Gets all groups. Can be filtered with FilterModel and Ordered with order model
    /// </summary>
    /// <param name="orderModel">provides order for list of groups</param>
    /// <param name="filterModel">provides filters for filtering groups</param>
    /// <returns>List of groups</returns>
    [HttpGet("")]
    [QueryValuesReader<DefaultOrderOptions>]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<GroupViewModel>>> GetAll(OrderModel? orderModel,[FilterModelBinder] FilterModel? filterModel)
    {
        var groups = await _groupsService.GetAll(orderModel, filterModel);

        return _mapper.Map<List<GroupViewModel>>(groups);
    }

    [HttpGet("{groupId:guid}")]
    public async Task<ActionResult<GroupViewModel>> Get(Guid groupId)
    {
        var group = await _groupsService.GetById(groupId);
        if (group is null)
            return Problem("Group was not found", statusCode:404);
        
        return _mapper.Map<GroupViewModel>(group);
    }

    /// <summary>
    /// Creates new group. 
    /// </summary>
    /// <param name="createGroupModel">group model to create</param>
    /// <returns>Created if group was successfully created otherwise returns problem</returns>
    [HttpPost("")]
    [Authorize(Roles = "Root,Admin")]
    public async Task<IActionResult> Create(CreateGroupModel createGroupModel)
    {
        var result = await _groupsService.Create(_mapper.Map<Group>(createGroupModel));

        if (result.IsSuccessful)
        {
            if (result.ResultObject is Group group)
                return CreatedAtAction("Get", new {groupId = group.Id});
            return Created();
        }
        
        if (result.Exception is AlreadyExistsException)
            return Problem("Group already exists in database", statusCode: 400, title: "Group already exists");

        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
            Detail = "An internal server error occurred while processing the request",
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Extensions = new Dictionary<string, object?>{{"errors", result.Errors}}
        });
    }

    [HttpPut("")]
    [Authorize(Roles = "Root,Admin")]
    public async Task<IActionResult> Update([FromForm] UpdateGroupModel updateGroupModel)
    {
        var result = await _groupsService.Update(_mapper.Map<Group>(updateGroupModel));
        
        if (result.IsSuccessful) 
            return NoContent();
        
        if (result.Exception is AlreadyExistsException)
            return Problem("Group already exists", statusCode: 400);
        
        if (result.Exception is NotFoundException)
            return Problem("Group was not found", statusCode:404);
        
        return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
        {
            Detail = "An internal server error occurred while processing the request",
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Extensions = new Dictionary<string, object?>{{"errors", result.Errors}}
        });
    }

    [HttpDelete("{groupId:guid}")]
    [Authorize(Roles = "Root,Admin")]
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
            Extensions = new Dictionary<string, object?>{{"errors", result.Errors}}
        });
    }
}