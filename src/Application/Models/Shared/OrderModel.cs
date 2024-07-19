using System.Linq.Expressions;
using Application.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Application.Models.Shared;

public class OrderModel
{
    public string? OrderOption { get; set; }
    public string? OrderBy { get; set; }

    public IQueryable<T> Order<T>(IQueryable<T> queryable, IEnumerable<KeyValuePair<OrderModel, Func<IQueryable<T>, IQueryable<T>>>> orders)
    {
        ArgumentNullException.ThrowIfNull(queryable);
        foreach (var order in orders)
        {
            if (this.Equals(order.Key))
            {
                return order.Value(queryable);
            }
        }

        return queryable;
    }
    
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
        return obj is OrderModel orderModel && (OrderBy?.Equals(orderModel.OrderBy, StringComparison.CurrentCultureIgnoreCase) ?? orderModel.OrderBy == null)  &&
               (OrderOption?.Equals(orderModel.OrderOption, StringComparison.CurrentCultureIgnoreCase) ?? orderModel.OrderOption == null);
    }


    private const int DefaultHash = 17;
    private const int HashMove = 23;
    public override int GetHashCode()
    {
        unchecked // it doesn't check is the newValue > int.MaxValue
        {
            int hash = DefaultHash;
            hash = hash * HashMove + (OrderOption?.GetHashCode() ?? 0);
            hash = hash * HashMove + (OrderBy?.GetHashCode() ?? 0);
            return hash;
        }
    }
}