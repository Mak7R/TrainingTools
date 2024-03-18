using Contracts.ModelContracts;

namespace TrainingTools.ViewModels;

public class OrderModel : IOrder
{
    private string _orderBy = string.Empty;
    public string OrderBy
    {
        get => _orderBy;
        set
        {
            if (value == null) return;
            
            _orderBy = value;
        }
    }

    private string _orderOption = string.Empty;
    public string OrderOption
    {
        get => _orderOption;
        set
        {
            if (value == null) return;
            
            _orderOption = value;
        }
    }
}