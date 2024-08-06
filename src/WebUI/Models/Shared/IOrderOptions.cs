namespace WebUI.Models.Shared;

public interface IOrderOptions
{
    public string Current { get; }
    public IOrderOptions Set(string? current);
    public IOrderOptions MoveNext();
}