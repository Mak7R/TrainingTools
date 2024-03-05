using Contracts.Models;

namespace Services;


public interface IViewCollectionBuilder<out T>
{
    IViewCollectionBuilder<T> Filter(IFilter filter);
    IViewCollectionBuilder<T> Order(IOrder order);
    IEnumerable<T> Build();
}