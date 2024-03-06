using Contracts.ModelContracts;
using Contracts.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TrainingTools.Models;

public class OrderModel : IOrder
{
    private string _orderBy = string.Empty;
    public string OrderBy
    {
        get => _orderBy;
        set
        {
            if (value == null) return;
            else _orderBy = value;
        }
    }

    private string _orderOption = string.Empty;
    public string OrderOption
    {
        get => _orderOption;
        set
        {
            if (value == null) return;
            else _orderOption = value;
        }
    }
}