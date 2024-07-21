using System.Net.Mime;
using Application.Interfaces.Services;
using Application.Models.Shared;
using AutoMapper;
using Domain.Enums;
using Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Models.Shared;
using WebUI.Models.User;

namespace WebUI.Controllers;


[Controller]
[Route("users")]
[Authorize]
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
    public async Task<IActionResult> GetAll(OrderModel? orderModel, FilterModel? filterModel, PageModel? pageModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = "/users"});
        
        pageModel ??= new PageModel();
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
    [Authorize(Roles = "Admin,Root")]
    [ConfirmUser]
    public async Task<IActionResult> GetAllAsCsv()
    {
        var stream = await _usersService.GetAllUsersAsCsv();

        return File(stream, MediaTypeNames.Text.Csv, "users.csv");
    }

    [HttpGet("{userName}")]
    [ConfirmUser]
    public async Task<IActionResult> Get(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return this.BadRequestView(new [] {"UserName was empty"});
        
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = $"/users/{userName}"});

        if (user.UserName == userName)
            return RedirectToAction("Profile", "Accounts");
        
        var userInfo = await _usersService.GetByName(user, userName);
        if (userInfo is null) return this.NotFoundView("User with this username was not found");
        
        return View(_mapper.Map<UserInfoViewModel>(userInfo));
    }
    
    [Authorize(Roles = "Admin,Root")]
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();
    }
    
    [Authorize(Roles = "Admin,Root")]
    [ConfirmUser]
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateUserModel createUserModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = "/users/create"});
        
        if (!ModelState.IsValid)
            return View(createUserModel);

        var appCreateUserDto = new Application.Dtos.CreateUserDto
        {
            Username = createUserModel.UserName,
            Email = createUserModel.Email,
            Phone = createUserModel.Phone,
            IsPublic = createUserModel.IsPublic,
            IsAdmin = createUserModel.IsAdmin,
            Password = createUserModel.Password
        };
        
        var result = await _usersService.Create(user, appCreateUserDto);

        if (!result.IsSuccessful)
            this.BadRequestView(result.Errors);

        return RedirectToAction("Get", new {userName = createUserModel.UserName});
    }

    [Authorize(Roles = "Admin,Root")]
    [HttpGet("{userName}/update")]
    public async Task<IActionResult> Update(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return this.BadRequestView(new[] { "User name was empty" });
        
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = $"/users/{userName}/update"});
        
        var user = await _usersService.GetByName(currentUser, userName);
        if (user is null) return this.NotFoundView("User was not found");
        
        var updateUserModel= new UpdateUserModel
        {
            UserName = userName,
            IsAdmin = user.Roles.Contains(nameof(Role.Admin)),
            ClearAbout = false,
            SetPrivate = false,
            IsTrainer = user.Roles.Contains(nameof(Role.Trainer))
        };
        
        return View(updateUserModel);
    }
    
    [Authorize(Roles = "Admin,Root")]
    [ConfirmUser]
    [HttpPost("{userName}/update")]
    public async Task<IActionResult> Update([FromRoute] string? userName, [FromForm] UpdateUserModel updateUserModel)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return this.BadRequestView(new[] { "User name was empty" });
        
        var user = await _userManager.GetUserAsync(User);
        if (user is null) 
            return RedirectToAction("Login", "Accounts", new {ReturnUrl = $"/users/{userName}/update"});
        
        if (!ModelState.IsValid)
        {
            updateUserModel.UserName = userName;
            return View(updateUserModel);
        }
        
        var appUpdateUserDto = new Application.Dtos.UpdateUserDto
        {
            UserName = userName,
            ClearAbout = updateUserModel.ClearAbout,
            IsAdmin = updateUserModel.IsAdmin,
            SetPrivate = updateUserModel.SetPrivate,
            IsTrainer = updateUserModel.IsTrainer  
        };
        
        var result = await _usersService.Update(user, appUpdateUserDto);
        
        if (!result.IsSuccessful)
            this.BadRequestView(result.Errors);

        return RedirectToAction("Get", new { userName });
    }

    [Authorize(Roles = "Admin,Root")]
    [ConfirmUser]
    [HttpGet("{userName}/delete")]
    public async Task<IActionResult> Delete([FromRoute] string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return this.BadRequestView(new[] { "User name was empty" });
        
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Accounts", new {ReturnUrl = "/users"});
        
        var result = await _usersService.Delete(user, userName);
        if (!result.IsSuccessful) return this.BadRequestView(result.Errors);
        return RedirectToAction("GetAll", "Users");
    }
}