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
}