namespace Contracts.Models;

public interface IOrder
{
    public string? OrderBy { get; }
    public string? OrderOption { get; }
}