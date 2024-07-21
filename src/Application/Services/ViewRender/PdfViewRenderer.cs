using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace Application.Services.ViewRender;


public class PdfViewRenderer : AbstractRazorViewRenderer<PdfOptions>
{
    public PdfViewRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IActionContextAccessor actionContextAccessor) : base(viewEngine, tempDataProvider, actionContextAccessor)
    {
    }

    public override async Task<Stream> RenderViewToStreamAsync<TModel>(string viewName, TModel model, PdfOptions? options)
    {
        var html = await RenderViewToStringAsync(viewName, model);
        var memoryStream = new MemoryStream();
        var pdf = PdfGenerator.GeneratePdf(html, PageSize.A4);
        pdf.Save(memoryStream, closeStream: false);
        return memoryStream;
    }
}

public class PdfOptions
{
}