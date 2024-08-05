using Microsoft.AspNetCore.Mvc;

namespace WebUI.Extensions;

public static class DefaultResponseExtensions
{
    public static IActionResult BadRequestRedirect(this Controller controller, IEnumerable<string> messages)
    {
        return controller.RedirectToAction("Info", "Home", new { statusCode = 400, messages });
    }

    public static IActionResult NotFoundRedirect(this Controller controller, IEnumerable<string> messages)
    {
        return controller.RedirectToAction("Info", "Home", new { statusCode = 404, messages });
    }
    
    public static IActionResult ErrorRedirect(this Controller controller, int statusCode, IEnumerable<string> messages)
    {
        return controller.RedirectToAction("Info", "Home", new { statusCode, messages });
    }

    public static IActionResult InfoRedirect(this Controller controller, int statusCode, IEnumerable<string> messages)
    {
        return controller.RedirectToAction("Info", "Home", new { statusCode, messages });
    }

    public static IActionResult ForbiddenRedirect(this Controller controller, IEnumerable<string> messages)
    {
        return controller.RedirectToAction("Info", "Home", new { statusCode = 403, messages });
    }
}