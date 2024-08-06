using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebUI.ModelBinding.ModelBinders;

namespace WebUI.ModelBinding.Providers;

public class FilterModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Metadata.ModelType.IsAssignableTo(typeof(FilterModel))) return new FilterModelBinderAttribute();

        return null;
    }
}