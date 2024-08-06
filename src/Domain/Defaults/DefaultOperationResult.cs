using Domain.Models;

namespace Domain.Defaults;

public class DefaultOperationResult : OperationResult
{
    public DefaultOperationResult(IEnumerable<string>? errors) : this(false, null, null, errors)
    {
    }

    public DefaultOperationResult(Exception exception, IEnumerable<string>? errors = null) : this(false, null,
        exception, errors)
    {
    }

    public DefaultOperationResult(object resultObject) : this(true, resultObject)
    {
    }

    public DefaultOperationResult(bool isSuccessful, object? resultObject = null, Exception? exception = null,
        IEnumerable<string>? errors = null)
    {
        IsSuccessful = isSuccessful;
        ResultObject = resultObject;
        Exception = exception;
        Errors = errors ?? Array.Empty<string>();
    }

    public override bool IsSuccessful { get; }
    public override object? ResultObject { get; }
    public override Exception? Exception { get; }
    public override IEnumerable<string> Errors { get; }

    public static DefaultOperationResult FromException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new DefaultOperationResult(exception, new[] { exception.Message });
    }
}