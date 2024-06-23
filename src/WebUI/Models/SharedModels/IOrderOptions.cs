namespace WebUI.Models.SharedModels;

public interface IOrderOptions
{
    public IOrderOptions Set(string? current);
    public IOrderOptions MoveNext();

    public string Current { get; }
}