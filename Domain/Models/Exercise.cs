namespace Domain.Models;

public class Exercise
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Group? Group { get; set; }
}