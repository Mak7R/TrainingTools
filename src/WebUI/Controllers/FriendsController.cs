using Application.Interfaces.Services;
using AutoMapper;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Filters;
using WebUI.Models.Friend;

namespace WebUI.Controllers;


[Controller]
[AuthorizeVerifiedRoles]
[Route("friends")]
public class FriendsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFriendsService _friendsService;
    private readonly ILogger<FriendsController> _logger;
    private readonly IMapper _mapper;

    public FriendsController(UserManager<ApplicationUser> userManager, IFriendsService friendsService, ILogger<FriendsController> logger, IMapper mapper)
    {
        _userManager = userManager;
        _friendsService = friendsService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet("invite")]
    public async Task<IActionResult> Invite(string? userName, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(userName)) 
            return this.BadRequestRedirect(new[] { "User name was invalid" });

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts");

        var targetUser = await _userManager.FindByNameAsync(userName);
        if (targetUser is null) return this.NotFoundRedirect(["User was not found"]);

        var result = await _friendsService.CreateInvitation(user, targetUser);

        if (!result.IsSuccessful)
        {
            _logger.LogWarning("Create invitation was unsuccessful. Errors: {errors}", string.Join("; ", result.Errors));
        }
        
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("accept")]
    public async Task<IActionResult> Accept(string? userName, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(userName)) 
            return this.BadRequestRedirect(new[] { "User name was invalid" });

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts");

        var invitor = await _userManager.FindByNameAsync(userName);
        if (invitor is null) return this.NotFoundRedirect(["User was not found"]);

        var result = await _friendsService.AcceptInvitation(invitor, user);
        
        if (!result.IsSuccessful)
        {
            _logger.LogWarning("Accept invitation was unsuccessful. Errors: {errors}", string.Join("; ", result.Errors));
        }
        
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet("refuse")]
    public async Task<IActionResult> Refuse(string? userName, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(userName)) 
            return this.BadRequestRedirect(new[] { "User name was invalid" });

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts");

        var invitor = await _userManager.FindByNameAsync(userName);
        if (invitor is null) return this.NotFoundRedirect(["User was not found"]);

        var result = await _friendsService.RemoveInvitation(invitor, user);
        
        if (!result.IsSuccessful)
        {
            _logger.LogWarning("Refuse invitation was unsuccessful. Errors: {errors}", string.Join("; ", result.Errors));
        }
        
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("cancel")]
    public async Task<IActionResult> Cancel(string? userName, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(userName)) 
            return this.BadRequestRedirect(new[] { "User name was invalid" });

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts");

        var target = await _userManager.FindByNameAsync(userName);
        if (target is null) return this.NotFoundRedirect(["User was not found"]);

        var result = await _friendsService.RemoveInvitation(user, target);
        
        if (!result.IsSuccessful)
        {
            _logger.LogWarning("Cancel invitation was unsuccessful. Errors: {errors}", string.Join("; ", result.Errors));
        }
        
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
    
    [HttpGet("remove")]
    public async Task<IActionResult> RemoveFriendship(string? userName, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(userName)) 
            return this.BadRequestRedirect(new[] { "User name was invalid" });

        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts");

        var friend = await _userManager.FindByNameAsync(userName);
        if (friend is null) return this.NotFoundRedirect(["User was not found"]);

        var result = await _friendsService.RemoveFriendship(user, friend);
        
        if (!result.IsSuccessful)
        {
            _logger.LogWarning("Remove friendship was unsuccessful. Errors: {errors}", string.Join("; ", result.Errors));
        }
        
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
    
    
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Accounts");

        var friends = await _friendsService.GetFriendsFor(user);
        var invitationsFor = await _friendsService.GetInvitationsFor(user);
        var invitationsOf = await _friendsService.GetInvitationsOf(user);

        return View(_mapper.Map<FriendRelationshipsInfoViewModel>((friends, invitationsFor, invitationsOf)));
    }
}