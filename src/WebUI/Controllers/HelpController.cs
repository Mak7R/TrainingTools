using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WebUI.Controllers;

[Route("help")]
[Controller]
[AllowAnonymous]
public class HelpController : Controller
{
    public HelpController()
    {
        
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}