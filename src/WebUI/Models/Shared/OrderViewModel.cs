using Application.Constants;
using Application.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Models.Shared;

public class OrderViewModel : OrderModel
{
    [FromQuery(Name = OrderOptionNames.Shared.OrderBy)]
    public override string? OrderBy { get; set; }

    [FromQuery(Name = OrderOptionNames.Shared.Order)]
    public override string? OrderOption { get; set; }
}