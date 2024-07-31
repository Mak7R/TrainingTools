using System.Net.Mime;
using Application.Dtos;
using Application.Interfaces.Services;
using Application.Models.Shared;
using AutoMapper;
using Domain.Enums;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Models.Shared;
using WebUI.Models.User;

namespace WebUI.Controllers;


[Controller]
[Route("users")]
public class UsersController : Controller
{
    private readonly IUsersService _usersService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public UsersController(IUsersService usersService, UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _usersService = usersService;
        _userManager = userManager;
        _mapper = mapper;
    }
    
    [HttpGet("")]
    [QueryValuesReader<DefaultOrderOptions>]
    [AuthorizeVerifiedRoles]
    public async Task<IActionResult> GetAll(FilterViewModel? filterModel, OrderViewModel? orderModel, PageViewModel? pageModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account", new {ReturnUrl = "/users"});
        
        pageModel ??= new PageViewModel();
        if (pageModel.PageSize is PageModel.DefaultPageSize or <= 0)
        {
            int defaultPageSize = 10;
            pageModel.PageSize = defaultPageSize;
            ViewBag.DefaultPageSize = defaultPageSize;
        } 
        ViewBag.UsersCount = (await _usersService.Count(user, filterModel)) - 1;
        
        var userInfos = await _usersService.GetAll(user, filterModel, orderModel, pageModel);
        
        return View(userInfos.Select(userInfo => _mapper.Map<UserInfoViewModel>(userInfo)));
    }
    
    [HttpGet("as-csv")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    public async Task<IActionResult> GetAllAsCsv()
    {
        var csvFileStream = await _usersService.GetAllUsersAsCsv();
        return File(csvFileStream, MediaTypeNames.Text.Csv, "users.csv");
    }

    [HttpGet("{userName}")]
    [AuthorizeVerifiedRoles]
    public async Task<IActionResult> Get(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return this.BadRequestRedirect(new [] {"UserName was empty"});
        
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account", new {ReturnUrl = $"/users/{userName}"});

        if (user.UserName == userName)
            return RedirectToAction("Profile", "Account");
        
        var userInfo = await _usersService.GetByName(user, userName);
        if (userInfo is null) return this.NotFoundRedirect(["User with this username was not found"]);
        
        return View(_mapper.Map<UserInfoViewModel>(userInfo));
    }
    
    [HttpGet("{userId:guid}")]
    [AuthorizeVerifiedRoles]
    public async Task<IActionResult> Get(Guid userId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account", new {ReturnUrl = $"/users/{userId}"});

        if (user.Id == userId)
            return RedirectToAction("Profile", "Account");
        
        var userInfo = await _usersService.GetById(user, userId);
        if (userInfo is null) return this.NotFoundRedirect(["User with this id was not found"]);
        
        return View(_mapper.Map<UserInfoViewModel>(userInfo));
    }

    [HttpGet("{userId:guid}/update")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    public async Task<IActionResult> Update(Guid userId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null) return RedirectToAction("Login", "Account", new {ReturnUrl = $"/users/{userId}/update"});

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return this.NotFoundRedirect(["User was not found"]);

        var roles = await _userManager.GetRolesAsync(user);
        var updateUserModel= new UpdateUserModel
        {
            Id = userId,
            UserName = user.UserName,
            IsAdmin = roles.Contains(nameof(Role.Admin)),
            ClearAbout = false,
            SetPrivate = false,
            IsTrainer = roles.Contains(nameof(Role.Trainer))
        };
        
        return View(updateUserModel);
    }
    
    
    [HttpPost("{userId:guid}/update")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    public async Task<IActionResult> Update([FromRoute] Guid userId, [FromForm] UpdateUserModel updateUserModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) 
            return RedirectToAction("Login", "Account", new {ReturnUrl = $"/users/{userId}/update"});
        
        if (!ModelState.IsValid)
        {
            updateUserModel.Id = userId;
            updateUserModel.UserName = updateUserModel.UserName;
            return View(updateUserModel);
        }
        
        var appUpdateUserDto = new UpdateUserDto
        {
            UserId = userId,
            ClearAbout = updateUserModel.ClearAbout,
            IsAdmin = updateUserModel.IsAdmin,
            SetPrivate = updateUserModel.SetPrivate,
            IsTrainer = updateUserModel.IsTrainer  
        };
        
        var result = await _usersService.Update(user, appUpdateUserDto);
        
        if (!result.IsSuccessful)
            this.BadRequestRedirect(result.Errors);

        return RedirectToAction("Get", new { userId });
    }
    
    [HttpGet("{userId:guid}/delete")]
    [AuthorizeVerifiedRoles(Role.Root, Role.Admin)]
    public async Task<IActionResult> Delete([FromRoute] Guid userId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account", new {ReturnUrl = "/users"});
        
        var result = await _usersService.Delete(user, userId);
        if (!result.IsSuccessful) return this.BadRequestRedirect(result.Errors);
        return RedirectToAction("GetAll", "Users");
    }
}