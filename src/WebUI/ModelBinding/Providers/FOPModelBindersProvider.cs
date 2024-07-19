using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebUI.ModelBinding.ModelBinders;

namespace WebUI.ModelBinding.Providers;


/// <summary>
/// Model binder provider for such models as FilterModel, OrderModel, PageModel (FOP)
/// </summary>
public class FOPModelBindersProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(FilterModel))
        {
            return new FilterModelBinderAttribute();
        }
        
        if (context.Metadata.ModelType == typeof(OrderModel))
        {
            return new OrderModelBinderAttribute();
        }
        
        if (context.Metadata.ModelType == typeof(PageModel))
        {
            return new PageModelBinderAttribute();
        }

        return null;
    }
}