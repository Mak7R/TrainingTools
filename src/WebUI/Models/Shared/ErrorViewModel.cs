namespace WebUI.Models.Shared;

public class ErrorViewModel
{
    public int StatusCode { get; set; }
    public IEnumerable<string> Errors { get; set; } = [];
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}