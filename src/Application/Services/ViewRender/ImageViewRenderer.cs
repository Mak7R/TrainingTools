using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PuppeteerSharp;

namespace Application.Services.ViewRender;

public class ImageViewRenderer : AbstractRazorViewRenderer<ImageOptions>
{
    public ImageViewRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IActionContextAccessor actionContextAccessor) : base(viewEngine, tempDataProvider, actionContextAccessor)
    {
    }

    public override async Task<Stream> RenderViewToStreamAsync<TModel>(string viewName, TModel model, ImageOptions? options)
    {
        var html = await RenderViewToStringAsync(viewName, model);

        await new BrowserFetcher().DownloadAsync();
        using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
        {
            var page = await browser.NewPageAsync();
            await page.SetContentAsync(html);
            var screenshotBytes = await page.ScreenshotDataAsync();
            return new MemoryStream(screenshotBytes);
        }
    }
}

public class ImageOptions
{
}