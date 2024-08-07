﻿using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Application.Dtos;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using WebUI.Models.Shared;
using WebUI.Models.User;

namespace WebUI.Controllers.Api.v1;

[ApiVersion("1.0")]
public class UsersController : ApiController
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService, UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _usersService = usersService;
        _userManager = userManager;
        _mapper = mapper;
    }

    /// <summary>
    ///     Retrieves a list of all groups, with optional filtering, ordering, and pagination.
    /// </summary>
    /// <param name="filterModel">Supported filters: f_id, f_name, f_name-equals, f_role, f_relationships</param>
    /// <param name="orderModel">Supported orders: name, role, relationships</param>
    /// <param name="pageModel">Paging params: page, page-size</param>
    /// <returns>List of users</returns>
    [HttpGet("")]
    [QueryValuesReader<DefaultOrderOptions>]
    [AuthorizeVerifiedRoles]
    public async Task<IActionResult> GetAll(FilterViewModel? filterModel, OrderViewModel? orderModel,
        PageViewModel? pageModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Problem("User unauthorized", statusCode: StatusCodes.Status401Unauthorized);

        var userInfos = await _usersService.GetAll(user, filterModel, orderModel, pageModel);

        return Ok(userInfos.Select(userInfo => _mapper.Map<UserInfoViewModel>(userInfo)));
    }

    /// <summary>
    ///     Counts the total number of users based on optional filter criteria.
    /// </summary>
    /// <param name="filterModel">Supported filters: f_id, f_name, f_name-equals, f_role, f_relationships</param>
    /// <returns></returns>
    [HttpGet("count")]
    [QueryValuesReader<DefaultOrderOptions>]
    [AuthorizeVerifiedRoles]
    public async Task<IActionResult> Count(FilterViewModel? filterModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Problem("User unauthorized", statusCode: StatusCodes.Status401Unauthorized);

        return Ok(await _usersService.Count(user, filterModel));
    }

    /// <summary>
    ///     Gets all users in csv format
    /// </summary>
    /// <returns>csv file with all users</returns>
    [HttpGet("as-csv")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    public async Task<IActionResult> GetAllAsCsv()
    {
        var stream = await _usersService.GetAllUsersAsCsv();

        return File(stream, MediaTypeNames.Text.Csv, "users.csv");
    }

    /// <summary>
    ///     Gets user by its userName
    /// </summary>
    /// <param name="userName">userName of user</param>
    /// <returns>user</returns>
    [HttpGet("{userName}")]
    [AuthorizeVerifiedRoles]
    public async Task<IActionResult> Get([Required] string? userName)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Problem("User unauthorized", statusCode: StatusCodes.Status401Unauthorized);

        var userInfo = await _usersService.GetByName(user, userName);
        if (userInfo is null)
            return Problem("User with this username was not found", statusCode: 404, title: "Not found");

        return Ok(_mapper.Map<UserInfoViewModel>(userInfo));
    }

    /// <summary>
    ///     Gets user by its <see cref="userId" />
    /// </summary>
    /// <param name="userId">ID of user</param>
    /// <returns>user</returns>
    [HttpGet("{userId:guid}")]
    [AuthorizeVerifiedRoles]
    public async Task<IActionResult> Get(Guid userId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Problem("User unauthorized", statusCode: StatusCodes.Status401Unauthorized);

        var userInfo = await _usersService.GetById(user, userId);
        if (userInfo is null)
            return Problem("User with this username was not found", statusCode: 404, title: "Not found");

        return Ok(_mapper.Map<UserInfoViewModel>(userInfo));
    }

    /// <summary>
    ///     Updates user by <see cref="userId" />.
    /// </summary>
    /// <param name="userId">ID of user</param>
    /// <param name="updateUserModel">model for updating user</param>
    /// <returns>user if updated successfully otherwise problem</returns>
    [HttpPut("{userId:guid}")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    public async Task<IActionResult> Update([FromRoute] Guid userId, [FromForm] UpdateUserModel updateUserModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Problem("User unauthorized", statusCode: StatusCodes.Status401Unauthorized);

        var appUpdateUserDto = new UpdateUserDto
        {
            UserId = userId,
            ClearAbout = updateUserModel.ClearAbout,
            IsAdmin = updateUserModel.IsAdmin,
            SetPrivate = updateUserModel.SetPrivate,
            IsTrainer = updateUserModel.IsTrainer
        };

        var result = await _usersService.Update(user, appUpdateUserDto);

        if (result.IsSuccessful)
            return Ok(_mapper.Map<UserViewModel>(await _userManager.FindByIdAsync(userId.ToString())));

        if (result.Exception is AlreadyExistsException)
            return BadRequest(new ProblemDetails
            {
                Detail = "User with this name already exists", Status = 400, Title = "Already exists",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }
            });
        if (result.Exception is NotFoundException)
            return Problem("User with this username was not found", statusCode: 404, title: "Not found");

        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }

    /// <summary>
    ///     Deletes user by id
    /// </summary>
    /// <param name="userId">ID of user</param>
    /// <returns>ok if successful deleted</returns>
    [HttpDelete("{userId:guid}")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    public async Task<IActionResult> Delete([FromRoute] Guid userId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Problem("User unauthorized", statusCode: StatusCodes.Status401Unauthorized);

        var result = await _usersService.Delete(user, userId);

        if (result.IsSuccessful)
            return Ok();

        if (result.Exception is NotFoundException)
            return Problem("User with this username was not found", statusCode: 404, title: "Not found");

        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }
}