using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Controller]
[Route("help")]
[AllowAnonymous]
public class HelpController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}