using Application.Models.Shared;

namespace ApplicationTests;

public class FilterModelTests
{
    [Fact]
    public void FilterBy_FilteredSuccessful_WhenFiltersAppliedCorrectly()
    {
        const string filterName = "name";
        var filterModel = new FilterModel
        {
            { filterName, "special" }
        };


        var list = new []
        {
            new {name = "common1"}, 
            new {name = "special1"}, 
            new {name = "common2"}, 
            new {name = "special2"},
            new {name = "common3"}, 
            new {name = "special3"},
        };
        var newList = filterModel.FilterBy(list, filterName, value => model => model.name.Contains(value));
    }
}