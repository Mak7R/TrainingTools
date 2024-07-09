using Microsoft.AspNetCore.Mvc;

namespace WebUI.Extensions;

public static class DefaultResponseExtensions
{
    public static IActionResult BadRequestView(this Controller controller, IEnumerable<string> errors)
    {
        controller.Response.StatusCode = StatusCodes.Status400BadRequest;
        return controller.View(@"ErrorViews\BadRequest", errors);
    }
    
    public static IActionResult NotFoundView(this Controller controller, string error)
    {
        controller.Response.StatusCode = StatusCodes.Status404NotFound;
        return controller.View(@"ErrorViews\NotFound", new []{error});
    }

    public static IActionResult NotFoundView(this Controller controller, IEnumerable<string> errors)
    {
        controller.Response.StatusCode = StatusCodes.Status404NotFound;
        return controller.View(@"ErrorViews\NotFound", errors);
    }
    
    public static IActionResult ErrorView(this Controller controller, int statusCode, IEnumerable<string> errors)
    {
        controller.Response.StatusCode = statusCode;
        return controller.View(@"ErrorViews\Error", errors);
    }
}