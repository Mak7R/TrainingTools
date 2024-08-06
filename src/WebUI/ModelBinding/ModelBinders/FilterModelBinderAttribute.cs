using Application.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebUI.Models.Shared;

namespace WebUI.ModelBinding.ModelBinders;

[AttributeUsage(
    // Support method parameters in actions.
    AttributeTargets.Parameter |

    // Support properties on model DTOs.
    AttributeTargets.Property |

    // Support model types.
    AttributeTargets.Class |
    AttributeTargets.Enum |
    AttributeTargets.Struct)]
public class FilterModelBinderAttribute : FromQueryAttribute, IModelBinder
{
    private static readonly int PrefixLength = FilterOptionNames.Shared.FiltersPrefix.Length;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var filterModel = new FilterViewModel();

        foreach (var queryValue in bindingContext.HttpContext.Request.Query)
            if (queryValue.Key.StartsWith(FilterOptionNames.Shared.FiltersPrefix))
                filterModel.Add(queryValue.Key.Substring(PrefixLength).ToLower(), queryValue.Value);

        bindingContext.Result = ModelBindingResult.Success(filterModel);

        return Task.CompletedTask;
    }
}