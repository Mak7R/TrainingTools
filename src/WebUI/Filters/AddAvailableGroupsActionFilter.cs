using Application.Interfaces.ServiceInterfaces;
using Application.Models.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebUI.Mapping.Mappers;
using WebUI.Models.Group;

namespace WebUI.Filters;

public class AddAvailableGroupsActionFilter : IAsyncActionFilter
{
    private readonly IMapper _mapper;
    private readonly IGroupsService _groupsService;
    private static readonly OrderModel DefaultOrder = new () { OrderOption = "ASC", OrderBy = "name" };

    public AddAvailableGroupsActionFilter(IGroupsService groupsService, IMapper mapper)
    {
        _groupsService = groupsService;
        _mapper = mapper;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next.Invoke();
        
        if (context.Controller is Controller controller)
        {
            controller.ViewBag.AvailableGroups = _mapper.Map<List<GroupViewModel>>(await _groupsService.GetAll(new OrderModel{OrderOption = "ASC", OrderBy = "name"}));
        }
    }
}