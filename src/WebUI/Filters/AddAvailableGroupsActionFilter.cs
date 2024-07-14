using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebUI.Mappers;

namespace WebUI.Filters;

public class AddAvailableGroupsActionFilter : IAsyncActionFilter
{
    private readonly IGroupsService _groupsService;
    private static readonly OrderModel DefaultOrder = new () { OrderOption = "ASC", OrderBy = "name" };

    public AddAvailableGroupsActionFilter(IGroupsService groupsService)
    {
        _groupsService = groupsService;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next.Invoke();
        
        if (context.Controller is Controller controller)
        {
            controller.ViewBag.AvailableGroups = (await _groupsService.GetAll(DefaultOrder)).Select(g => g.ToGroupViewModel());
        }
    }
}