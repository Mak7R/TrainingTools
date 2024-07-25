using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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