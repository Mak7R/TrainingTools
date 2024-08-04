using Application.Interfaces.Services;
using AutoMapper;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Filters;
using WebUI.Models.Friend;
using WebUI.Models.User;

namespace WebUI.Controllers.Api.v1;


[AuthorizeVerifiedRoles]
public class FriendsController : ApiController
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

    /// <summary>
    /// Invite user to friendship. Creates friend invitation between current user and user with <see cref="userId"/>
    /// </summary>
    /// <param name="userId">represents id of user which is invited by current user</param>
    /// <returns>User id or error response</returns>
    [HttpPost("{userId:guid}/invite")]
    public async Task<IActionResult> Invite(Guid userId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        var targetUser = await _userManager.FindByIdAsync(userId.ToString());
        if (targetUser is null) return Problem(detail:"User was not found", statusCode:404, title:"Not found");

        var result = await _friendsService.CreateInvitation(user, targetUser);

        if (result.IsSuccessful)
            return Ok(userId);
        
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }

    /// <summary>
    /// Accepts invitation of user with id <see cref="userId"/>
    /// </summary>
    /// <param name="userId">represents id of user which has invited current user</param>
    /// <returns>User id or error response</returns>
    [HttpPut("{userId:guid}/accept")]
    public async Task<IActionResult> Accept(Guid userId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        var invitor = await _userManager.FindByIdAsync(userId.ToString());
        if (invitor is null) return Problem(detail:"User was not found", statusCode:404, title:"Not found");

        var result = await _friendsService.AcceptInvitation(invitor, user);
        
        if (result.IsSuccessful)
            return Ok(userId);
        
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }
    
    /// <summary>
    /// Refuses invitation of user with id <see cref="userId"/>
    /// </summary>
    /// <param name="userId">represents id of user which has invited current user</param>
    /// <returns>User id or error response</returns>
    [HttpDelete("{userId:guid}/refuse")]
    public async Task<IActionResult> Refuse(Guid userId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        var invitor = await _userManager.FindByIdAsync(userId.ToString());
        if (invitor is null) return Problem(detail:"User was not found", statusCode:404, title:"Not found");

        var result = await _friendsService.RemoveInvitation(invitor, user);
        
        if (result.IsSuccessful)
            return Ok(userId);
        
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }

    /// <summary>
    /// Cancels invitation to user with id <see cref="userId"/>
    /// </summary>
    /// <param name="userId">represents id of user which was invited by current user</param>
    /// <returns>User id or error response</returns>
    [HttpDelete("{userId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid userId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        var target = await _userManager.FindByNameAsync(userId.ToString());
        if (target is null) return Problem(detail:"User was not found", statusCode:404, title:"Not found");

        var result = await _friendsService.RemoveInvitation(user, target);
        
        if (result.IsSuccessful)
            return Ok(userId);
        
        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }
    
    /// <summary>
    /// Removes friendship between current user and user with <see cref="userId"/>
    /// </summary>
    /// <param name="userId">represents user which is friend to current</param>
    /// <returns>User id or error response</returns>
    [HttpDelete("{userId:guid}/remove")]
    public async Task<IActionResult> RemoveFriendship(Guid userId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null)return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        var friend = await _userManager.FindByNameAsync(userId.ToString());
        if (friend is null) return Problem(detail:"User was not found", statusCode:404, title:"Not found");

        var result = await _friendsService.RemoveFriendship(user, friend);

        if (result.IsSuccessful)
            return Ok(userId);

        return StatusCode(500,
            new ProblemDetails
            {
                Detail = "Server error was occurred while processing the request",
                Extensions = new Dictionary<string, object?> { { "errors", result.Errors } }, Status = 500,
                Title = "Server error"
            });
    }
    
    /// <summary>
    /// Get all friends for current user 
    /// </summary>
    /// <returns>List of friends</returns>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);

        var friends = await _friendsService.GetFriendsFor(user);

        return Ok(_mapper.Map<UserViewModel>(friends));
    }
    
    /// <summary>
    /// Get all invitations for current user (current user is invited)
    /// </summary>
    /// <returns>List of invitations where user is invited</returns>
    [HttpGet("invitations-for")]
    public async Task<IActionResult> InvitationsFor()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);
        
        var invitationsFor = await _friendsService.GetInvitationsFor(user);

        return Ok(_mapper.Map<FriendInvitationViewModel>(invitationsFor));
    }
    
    /// <summary>
    /// Get all invitations of current user (current user is invitor)
    /// </summary>
    /// <returns>List of invitations where user is invitor</returns>
    [HttpGet("invitations-of")]
    public async Task<IActionResult> InvitationsOf()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Problem("User unauthorized", statusCode:StatusCodes.Status401Unauthorized);
        
        var invitationsOf = await _friendsService.GetInvitationsOf(user);

        return Ok(_mapper.Map<FriendInvitationViewModel>(invitationsOf));
    }
}