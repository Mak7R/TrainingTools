using Application.Constants;
using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebUI.ModelBinding.ModelBinders;

public class OrderModelBinderAttribute : FromQueryAttribute, IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        OrderModel? orderModel = null;
        foreach (var queryValue in bindingContext.HttpContext.Request.Query)
        {
            if (queryValue.Key == OrderOptionNames.Shared.OrderBy)
            {
                orderModel ??= new OrderModel{OrderOption = string.Empty};
                orderModel.OrderBy = queryValue.Value;
            }

            if (queryValue.Key == OrderOptionNames.Shared.Order)
            {
                orderModel ??= new OrderModel{OrderBy = string.Empty};
                orderModel.OrderOption = queryValue.Value;
            }
        }
        
        bindingContext.Result = ModelBindingResult.Success(orderModel);
        
        return Task.CompletedTask;
    }
}