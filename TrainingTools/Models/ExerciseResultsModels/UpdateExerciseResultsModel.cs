namespace TrainingTools.Models;

public class UpdateExerciseResultsModel
{
    public Guid ExerciseResultsId { get; set; }
    public List<Entry> ExerciseResultsEntries { get; set; }

    public record Entry(int Count, int Weight);
}