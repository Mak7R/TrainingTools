namespace WebUI.Models.ExerciseResult;

public class ApproachViewModel(decimal weight, int count, string? comment)
{
    public decimal Weight { get; set; } = weight;
    public int Count { get; set; } = count;
    public string? Comment { get; set; } = comment;

    public ApproachViewModel() : this(0, 0, string.Empty)
    {
    }
}