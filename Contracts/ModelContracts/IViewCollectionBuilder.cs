namespace Contracts.ModelContracts;


public interface IViewCollectionBuilder<out T>
{
    IViewCollectionBuilder<T> Filter(IFilter filter);
    IViewCollectionBuilder<T> Order(IOrder order);
    IEnumerable<T> Build();
}