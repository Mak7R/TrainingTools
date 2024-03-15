using System.Text;
using TrainingTools.ViewModels;

namespace TrainingTools.Components;

public class RequestCollectionsHelper
{
    public OrderModel? Order { get; private set; }
    public FilterModel? Filter { get; private set; }

    public event Action<string> OnSet = _ => { };
    public event Action<OrderModel> OnSetOrder = _ => { };
    public event Action<FilterModel> OnSetFilter = _ => { };

    private string _queryString
    {
        get
        {
            var stringBuilder = new StringBuilder();
            if (Filter != null)
            {
                stringBuilder.Append("FilterBy=");
                stringBuilder.Append(Filter.FilterBy);
                stringBuilder.Append("&FilterValue=");
                stringBuilder.Append(Filter.FilterValue);
                if (Order != null)
                {
                    stringBuilder.Append("&OrderBy=");
                    stringBuilder.Append(Order.OrderBy);
                    stringBuilder.Append("&OrderOption=");
                    stringBuilder.Append(Order.OrderOption);
                }
            }
            else
            {
                if (Order != null)
                {
                    stringBuilder.Append("OrderBy=");
                    stringBuilder.Append(Order.OrderBy);
                    stringBuilder.Append("&OrderOption=");
                    stringBuilder.Append(Order.OrderOption);
                }
            }

            return stringBuilder.ToString();
        }
    }

    public void SetOrder(OrderModel order)
    {
        Order = order;
        OnSetOrder(order);
        OnSet(_queryString);
    }

    public void SetFilter(FilterModel filter)
    {
        Filter = filter;
        OnSetFilter(filter);
        OnSet(_queryString);
    }
}