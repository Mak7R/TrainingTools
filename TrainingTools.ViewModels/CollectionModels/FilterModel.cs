using Contracts.ModelContracts;

namespace TrainingTools.ViewModels;

public class FilterModel : IFilter
{
    private string _filterBy = string.Empty;
    public string FilterBy
    {
        get => _filterBy;
        set
        {
            if (value == null) return;
            
            _filterBy = value;
        }
    }

    private string _filterValue = string.Empty;
    public string FilterValue
    {
        get => _filterValue;
        set
        {
            if (value == null) return;
            
            _filterValue = value;
        }
    }
}