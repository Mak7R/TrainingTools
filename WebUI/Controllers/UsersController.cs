using System.Net.Mime;
using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Domain.Enums;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.ModelBinding.CustomModelBinders;
using WebUI.Models.SharedModels;
using WebUI.Models.UserModels;

namespace WebUI.Controllers;


[Controller]
[Route("users")]
[Authorize]
public class UsersController : Controller
{
    private readonly IUsersService _usersService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(IUsersService usersService, UserManager<ApplicationUser> userManager)
    {
        _usersService = usersService;
        _userManager = userManager;
    }
    
    [HttpGet("")]
    [TypeFilter(typeof(QueryValuesProvidingActionFilter), Arguments = new object[] { typeof(DefaultOrderOptions) })]
    public async Task<IActionResult> GetAllUsers([FromQuery] OrderModel? orderModel, [ModelBinder(typeof(FilterModelBinder))]FilterModel? filterModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = "/users"});
        
        var userInfos = await _usersService.GetAllUsers(user, orderModel, filterModel);

        return View(userInfos);
    }
    
    [HttpGet("as-csv")]
    [Authorize(Roles = "Admin,Root")]
    public async Task<IActionResult> GetAllUsersAsCsv()
    {
        var stream = await _usersService.GetAllUsersAsCsv();

        return File(stream, MediaTypeNames.Text.Csv, "users.csv");
    }

    [HttpGet("{userName}")]
    public async Task<IActionResult> GetUser(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return this.BadRequestView(new [] {"UserName was empty"});
        
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = $"/users/{userName}"});

        if (user.UserName == userName)
            return RedirectToAction("Profile", "Accounts");
        
        var userInfo = await _usersService.GetByName(user, userName);
        if (userInfo is null) return this.NotFoundView("User with this username was not found");

        return View(userInfo);
    }
    
    [Authorize(Roles = "Admin,Root")]
    [HttpGet("create")]
    public IActionResult CreateUser()
    {
        return View();
    }
    
    [Authorize(Roles = "Admin,Root")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = "/users/create"});
        
        if (!ModelState.IsValid)
            return View(createUserDto);

        var appCreateUserDto = new Application.Dtos.CreateUserDto
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            Phone = createUserDto.Phone,
            IsPublic = createUserDto.IsPublic,
            IsAdmin = createUserDto.IsAdmin,
            Password = createUserDto.Password
        };
        
        var result = await _usersService.CreateUser(user, appCreateUserDto);

        if (!result.IsSuccessful)
        {
            this.BadRequestView(result.Errors);
        }

        return RedirectToAction("GetUser", new {userName = createUserDto.Username});
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpGet("{userId:guid}/update")]
    public async Task<IActionResult> UpdateUser(Guid userId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = $"/users/{userId}/update"});
        
        var user = await _usersService.GetById(currentUser, userId);
        if (user is null) return this.NotFoundView("User was not found");
        
        var updateUserDto = new UpdateUserDto
        {
            Username = user.User.UserName,
            IsAdmin = user.Roles.Contains(nameof(Role.Admin)),
            ClearAbout = false,
            SetPrivate = false,
            IsTrainer = user.Roles.Contains(nameof(Role.Trainer))
        };
        
        return View(updateUserDto);
    }
    
    [Authorize(Roles = "Admin,Root")]
    [HttpPost("{userId:guid}/update")]
    public async Task<IActionResult> UpdateUser(Guid userId, UpdateUserDto updateUserDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = $"/users/{userId}/update"});
        
        if (!ModelState.IsValid)
            return View(updateUserDto);
        
        var appUpdateUserDto = new Application.Dtos.UpdateUserDto
        {
            UserId = userId,
            Username = updateUserDto.Username,
            ClearAbout = updateUserDto.ClearAbout,
            IsAdmin = updateUserDto.IsAdmin,
            SetPrivate = updateUserDto.SetPrivate,
            IsTrainer = updateUserDto.IsTrainer  
        };
        
        var result = await _usersService.UpdateUser(user, appUpdateUserDto);
        
        if (!result.IsSuccessful)
        {
            this.BadRequestView(result.Errors);
        }

        return RedirectToAction("GetUser", new {userName = appUpdateUserDto.Username});
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpGet("{userId:guid}/delete")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid userId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = "/users"});
        
        var result = await _usersService.DeleteUser(user, userId);
        if (!result.IsSuccessful) return this.BadRequestView(result.Errors);
        return RedirectToAction("GetAllUsers");
    }
}