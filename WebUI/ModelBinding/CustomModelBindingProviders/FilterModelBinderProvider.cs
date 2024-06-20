using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using WebUI.ModelBinding.CustomModelBinders;

namespace WebUI.ModelBinding.CustomModelBindingProviders;

public class FilterModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(FilterModel))
        {
            return new BinderTypeModelBinder(typeof(FilterModelBinder));
        }

        return null;
    }
}