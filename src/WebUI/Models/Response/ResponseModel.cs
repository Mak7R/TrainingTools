namespace WebUI.Models.Response;

public class ResponseModel
{
    public ResponseModel(bool isSuccessful = false, object? result = null, IEnumerable<string>? errors = null)
    {
        IsSuccessful = IsSuccessful;
        Result = result;
        Errors = Errors ?? [];
    }

    public bool IsSuccessful { get; set; }
    public object? Result { get; set; }
    public IEnumerable<string> Errors { get; set; }

    public static ResponseModel BadResponse(IEnumerable<string> errors)
    {
        return new ResponseModel(false, null, errors);
    }

    public static ResponseModel GoodResponse(object? result)
    {
        return new ResponseModel(true, result);
    }
}