using Application.Interfaces.Services;
using Application.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebUI.Models.Group;

namespace WebUI.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AddAvailableGroupsAttribute : ActionFilterAttribute
{
    private static readonly OrderModel DefaultOrder = new () { OrderOption = "ASC", OrderBy = "name" };
    
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next.Invoke();
        
        if (context.Controller is Controller controller)
        {
            var groupsService = controller.HttpContext.RequestServices.GetRequiredService<IGroupsService>();
            var mapper = controller.HttpContext.RequestServices.GetRequiredService<IMapper>();
            controller.ViewBag.AvailableGroups = mapper.Map<List<GroupViewModel>>(await groupsService.GetAll(DefaultOrder));
        }
    }
}