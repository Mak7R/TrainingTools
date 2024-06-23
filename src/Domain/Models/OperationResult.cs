namespace Domain.Models;

public abstract class OperationResult
{
    public abstract bool IsSuccessful { get; }
    public abstract object? ResultObject { get; }
    public abstract Exception? Exception { get; }
    public abstract IEnumerable<string> Errors { get; }
}