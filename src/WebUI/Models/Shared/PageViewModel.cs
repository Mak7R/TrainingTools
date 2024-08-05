using Application.Constants;
using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Models.Shared;

public class PageViewModel : PageModel
{
    [FromQuery(Name = PagingOptionNames.CurrentPage)]
    public override int CurrentPage { get; set; }

    [FromQuery(Name = PagingOptionNames.PageSize)]
    public override int PageSize { get; set; }
}