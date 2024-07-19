using Application.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebUI.Models.Shared;

namespace WebUI.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class QueryValuesReaderAttribute<TOrderOptions> : ActionFilterAttribute
    where TOrderOptions: IOrderOptions
{
    private readonly TOrderOptions _orderOptions = (TOrderOptions)(Activator.CreateInstance(typeof(TOrderOptions)) 
                                                                   ?? throw new NullReferenceException("Null result of creating order options Instance"));

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Controller is not Controller controller) return;
        
        foreach (var queryValues in context.HttpContext.Request.Query)
        {
            var key = queryValues.Key;
            var value = queryValues.Value.First();

            if (key.Equals(OrderOptionNames.Shared.Order, StringComparison.CurrentCultureIgnoreCase))
            {
                controller.ViewData["current_order"] = _orderOptions.Set(value).Current;
                controller.ViewData[key] = _orderOptions.MoveNext().Current;
            }
                
            else if (!string.IsNullOrWhiteSpace(value) && 
                     (key.Equals(OrderOptionNames.Shared.OrderBy, StringComparison.CurrentCultureIgnoreCase) || 
                      key.Equals("page", StringComparison.CurrentCultureIgnoreCase) || 
                      key.StartsWith("f_"))
                    )
            {
                controller.ViewData[key] = value;
            }
        }
    }
}