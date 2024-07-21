namespace Application.Interfaces.Services;

public interface IViewRenderer<in TOptions>
{
    Task<Stream> RenderViewToStreamAsync<TModel>(string viewName, TModel model, TOptions options);
}