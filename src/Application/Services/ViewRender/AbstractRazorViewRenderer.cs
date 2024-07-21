using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Application.Services.ViewRender;

public abstract class AbstractRazorViewRenderer<TOptions> : IViewRenderer<TOptions>
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly ActionContext _actionContext;

    public AbstractRazorViewRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IActionContextAccessor actionContextAccessor)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _actionContext = actionContextAccessor.ActionContext;
    }
    public abstract Task<Stream> RenderViewToStreamAsync<TModel>(string viewName, TModel model, TOptions? options);
    
    protected async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
    {
        var viewResult = _viewEngine.FindView(_actionContext, viewName, false);
        using (var stringWriter = new StringWriter())
        {
            var viewContext = new ViewContext(_actionContext, viewResult.View, new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model }, new TempDataDictionary(_actionContext.HttpContext, _tempDataProvider), stringWriter, new HtmlHelperOptions());
            await viewResult.View.RenderAsync(viewContext);
            return stringWriter.ToString();
        }
    }
}