using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using WebUI.Extensions;
using WebUI.Models.Shared;

namespace WebUI.Controllers;


[Controller]
[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    
    [Route("/")]
    public IActionResult Index()
    {
        return View();
    }

    [Route("/privacy")]
    public IActionResult Privacy()
    {
        return View();
    }
    
    [HttpGet("/set-lang")]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        try
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
        }
        catch (Exception e)
        {
            return this.BadRequestRedirect(new []{e.Message});
        }
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) 
            return LocalRedirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [Route("/info")]
    [Route("/info/{statusCode:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Info(int statusCode = 500, string[]? messages = null)
    {
        if (statusCode is >= 200 and < 600)
        {
            Response.StatusCode = statusCode;
        }
        
        return View("Info", new InfoViewModel
        {
            StatusCode = statusCode, 
            Messages = messages ?? [], 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
    
    [Route("/error")]
    [Route("/error/{statusCode:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int statusCode = 500, string[]? messages = null)
    {
        if (statusCode is >= 200 and < 600)
        {
            Response.StatusCode = statusCode;
        }
        
        return View("Info", new InfoViewModel
        {
            StatusCode = statusCode, 
            Messages = messages ?? [], 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}