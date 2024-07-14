namespace WebUI.Models.ResponseModels;

public class ResponseModel
{
    public bool IsSuccessful { get; set; }
    public object? Result { get; set; }
    public IEnumerable<string> Errors { get; set; }

    public ResponseModel(bool isSuccessful = false, object? result = null, IEnumerable<string>? errors = null)
    {
        IsSuccessful = IsSuccessful;
        Result = result;
        Errors = Errors ?? [];
    }

    public static ResponseModel BadResponse(IEnumerable<string> errors) => new (false, null, errors);
    public static ResponseModel GoodResponse(object? result) => new (true, result);
}