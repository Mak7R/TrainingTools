using Application.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Application.Models.Shared;

public class OrderModel
{
    [FromQuery(Name = OrderOptionNames.Shared.Order)] public string? OrderOption { get; set; }
    [FromQuery(Name = OrderOptionNames.Shared.OrderBy)] public string? OrderBy { get; set; }

    public IEnumerable<T> Order<T>(IEnumerable<T> enumerable, IEnumerable<KeyValuePair<OrderModel, Func<IEnumerable<T>, IEnumerable<T>>>> orders)
    {
        ArgumentNullException.ThrowIfNull(enumerable);
        foreach (var order in orders)
        {
            if (this.Equals(order.Key))
            {
                return order.Value(enumerable);
            }
        }

        return enumerable;
    }

    public override bool Equals(object? obj)
    {
        return obj is OrderModel orderModel && this.OrderBy == orderModel.OrderBy &&
               this.OrderOption == orderModel.OrderOption;
    }


    private const int DefaultHash = 17;
    private const int HashMove = 23;
    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = DefaultHash;
            hash = hash * HashMove + (OrderOption?.GetHashCode() ?? 0);
            hash = hash * HashMove + (OrderBy?.GetHashCode() ?? 0);
            return hash;
        }
    }
}