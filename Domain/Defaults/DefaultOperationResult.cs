using Domain.Models;

namespace Domain.Defaults;

public class DefaultOperationResult : OperationResult
{
    public override bool IsSuccessful { get; }
    public override object? ResultObject { get; }
    public override IEnumerable<string> Errors { get; }

    public DefaultOperationResult(bool isSuccessful, object? resultObject = null, IEnumerable<string>? errors = null)
    {
        IsSuccessful = isSuccessful;
        ResultObject = resultObject;
        Errors = errors ?? Array.Empty<string>();
    }

    public DefaultOperationResult(IEnumerable<string> errors)
    {
        IsSuccessful = false;
        ResultObject = null;
        Errors = errors;
    }
}