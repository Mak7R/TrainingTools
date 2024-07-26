using Microsoft.AspNetCore.Mvc;

namespace WebUI.Extensions;

public static class DefaultResponseExtensions
{
    public static IActionResult BadRequestRedirect(this Controller controller, IEnumerable<string> errors)
    {
        return controller.RedirectToAction("Error", "Home", new { statusCode = 400, errors });
    }

    public static IActionResult NotFoundRedirect(this Controller controller, IEnumerable<string> errors)
    {
        return controller.RedirectToAction("Error", "Home", new { statusCode = 404, errors });
    }
    
    public static IActionResult ErrorRedirect(this Controller controller, int statusCode, IEnumerable<string> errors)
    {
        return controller.RedirectToAction("Error", "Home", new { statusCode, errors });
    }

    public static IActionResult ForbiddenRedirect(this Controller controller, IEnumerable<string> errors)
    {
        return controller.RedirectToAction("Error", "Home", new { statusCode = 403, errors });
    }
}