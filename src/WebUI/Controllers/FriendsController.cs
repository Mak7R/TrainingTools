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

    [HttpGet("{userId:guid}/invite")]
    public async Task<IActionResult> Invite(Guid userId, string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Account");

        var targetUser = await _userManager.FindByIdAsync(userId.ToString());
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

    [HttpGet("{userId:guid}/accept")]
    public async Task<IActionResult> Accept(Guid userId, string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Account");

        var invitor = await _userManager.FindByIdAsync(userId.ToString());
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
    
    [HttpGet("{userId:guid}/refuse")]
    public async Task<IActionResult> Refuse(Guid userId, string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Account");

        var invitor = await _userManager.FindByIdAsync(userId.ToString());
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

    [HttpGet("{userId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid userId, string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Account");

        var target = await _userManager.FindByNameAsync(userId.ToString());
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
    
    [HttpGet("{userId:guid}/remove")]
    public async Task<IActionResult> RemoveFriendship(Guid userId, string? returnUrl)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return RedirectToAction("Login", "Account");

        var friend = await _userManager.FindByNameAsync(userId.ToString());
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
        if (user is null) return RedirectToAction("Login", "Account");

        var friends = await _friendsService.GetFriendsFor(user);
        var invitationsFor = await _friendsService.GetInvitationsFor(user);
        var invitationsOf = await _friendsService.GetInvitationsOf(user);

        return View(_mapper.Map<FriendRelationshipsInfoViewModel>((friends, invitationsFor, invitationsOf)));
    }
}