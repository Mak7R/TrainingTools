using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebUI.ModelBinding.CustomModelBinders;

class FilterModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);
        
        var filterModel = new FilterModel();

        foreach (var queryValue in bindingContext.HttpContext.Request.Query)
        {
            if (queryValue.Key.StartsWith("f_"))
            {
                filterModel.Add(queryValue.Key.Substring(2).ToLower(), queryValue.Value.First());
            }
        }
        
        bindingContext.Result = ModelBindingResult.Success(filterModel);
        
        return Task.CompletedTask;
    }
}