namespace WebUI.Models.Shared;

public class InfoViewModel
{
    public int StatusCode { get; set; }
    public IEnumerable<string> Messages { get; set; } = [];
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}