using Application.Interfaces.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;


[Controller]
[Route("users")]
[Authorize]
public class UsersController : Controller
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }
    
    public async Task<IActionResult> GetAllUsers()
    {
        var userInfos = await _usersService.GetAllUsers(HttpContext.User);

        return View(userInfos);
    }
    
    [HttpGet("create")]
    public IActionResult CreateUser()
    {
        throw new NotImplementedException();
    }

    [HttpGet("{userId:guid}")]
    public IActionResult GetUser()
    {
        throw new NotImplementedException();
    }

    [HttpGet("{userId:guid}/delete")]
    public IActionResult RemoveUser()
    {
        throw new NotImplementedException();
    }
    
}