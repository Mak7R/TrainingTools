using Microsoft.AspNetCore.Mvc;

namespace Application.Models.Shared;

public class OrderModel
{
    [FromQuery(Name = "order")] public string? Order { get; set; }
    [FromQuery(Name = "order_by")] public string? OrderBy { get; set; }
}