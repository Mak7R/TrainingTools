using Application.Constants;
using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebUI.ModelBinding.ModelBinders;

public class PageModelBinderAttribute : FromQueryAttribute, IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        PageModel? pageModel = null;
        foreach (var queryValue in bindingContext.HttpContext.Request.Query)
        {
            if (queryValue.Key == PagingOptionNames.CurrentPage && int.TryParse(queryValue.Value, out var currentPage))
            {
                pageModel ??= new PageModel();
                pageModel.CurrentPage = currentPage;
            }

            if (queryValue.Key == PagingOptionNames.PagesCount && int.TryParse(queryValue.Value, out var pagesCount))
            {
                pageModel ??= new PageModel();
                pageModel.PagesCount = pagesCount;
            }
            
            if (queryValue.Key == PagingOptionNames.PageSize && int.TryParse(queryValue.Value, out var pageSize))
            {
                pageModel ??= new PageModel();
                pageModel.PageSize = pageSize;
            }
        }
        
        bindingContext.Result = ModelBindingResult.Success(pageModel);
        
        return Task.CompletedTask;
    }
}