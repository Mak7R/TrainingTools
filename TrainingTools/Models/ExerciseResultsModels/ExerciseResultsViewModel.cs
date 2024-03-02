using Contracts.Models;

namespace TrainingTools.Models;

public class ExerciseResultsViewModel
{
    public Guid Id { get; set; }
    public IEnumerable<(Guid Id, int Count, int Weight)> Results { get; set; }

    public ExerciseResultsViewModel(ExerciseResults exerciseResults)
    {
        Id = exerciseResults.Id;
        Results = exerciseResults.Results.Select(r => (Guid.Empty, r.Count, r.Weight));
    }
}