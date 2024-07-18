using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebUI.ModelBinding.ModelBinders;

[AttributeUsage(
    // Support method parameters in actions.
    AttributeTargets.Parameter |

    // Support properties on model DTOs.
    AttributeTargets.Property |

    // Support model types.
    AttributeTargets.Class |
    AttributeTargets.Enum |
    AttributeTargets.Struct,

    AllowMultiple = false,
    Inherited = true)]
public class FilterModelBinderAttribute : Attribute, IModelBinder
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