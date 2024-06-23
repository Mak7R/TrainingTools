using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebUI.Models.SharedModels;

namespace WebUI.Filters;

public class QueryValuesProvidingActionFilter : IActionFilter
{
    private IOrderOptions _orderOptions;

    public QueryValuesProvidingActionFilter(Type orderOptionsType)
    {
        if (!typeof(IOrderOptions).IsAssignableFrom(orderOptionsType))
        {
            throw new ArgumentException("Type must implement interface IOrderOptions", nameof(orderOptionsType));
        }

        _orderOptions = (IOrderOptions)(Activator.CreateInstance(orderOptionsType) ?? throw new NullReferenceException("Null result of creating orederOptionsInstace"));
    }
    
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // nothing
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Controller is Controller controller)
        {
            foreach (var queryValues in context.HttpContext.Request.Query)
            {
                var key = queryValues.Key;
                var value = queryValues.Value.First();

                if (key.Equals(nameof(OrderModel.Order), StringComparison.CurrentCultureIgnoreCase))
                {
                    controller.ViewData["current_order"] = _orderOptions.Set(value).Current;
                    controller.ViewData[key] = _orderOptions.MoveNext().Current;
                }
                
                else if (!string.IsNullOrWhiteSpace(value) && 
                    (key.Equals("order_by", StringComparison.CurrentCultureIgnoreCase) || 
                     key.Equals("page", StringComparison.CurrentCultureIgnoreCase) || 
                     key.StartsWith("f_"))
                    )
                {
                    controller.ViewData[key] = value;
                }
            }
        }
        
    }
}