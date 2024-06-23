namespace Domain.Models;

public class Approach(decimal weight, int count, string? comment)
{
    public decimal Weight { get; set; } = weight;
    public int Count { get; set; } = count;
    public string? Comment { get; set; } = comment;

    public Approach() : this(0, 0, string.Empty)
    {
    }
}