using Microsoft.AspNetCore.Mvc;

namespace TrainingTools.Controllers;

[Route("/")]
public class HomeController : Controller
{
    [HttpGet]
    [Route("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Route("error")]
    public IActionResult Error()
    {
        return View((Response.StatusCode, "Internal server error"));
    }

    [HttpGet]
    [Route("error/{statusCode:int}")]
    public IActionResult Error([FromRoute] int statusCode, string? message)
    {
        if (string.IsNullOrEmpty(message)) message = "Custom users error";
        return View((statusCode, message));
    }
}