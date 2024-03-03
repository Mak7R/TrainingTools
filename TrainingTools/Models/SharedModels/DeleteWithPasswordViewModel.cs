namespace TrainingTools.Models;

public class DeleteWithPasswordViewModel
{
    public string ButtonName { get; set; }
    public string Name { get; set; }
    public string HandlerUrl { get; set; }
    
    /// <summary>
    /// Write: bsDeleteModal.hide(); to hide modal
    /// </summary>
    public string OnSuccess { get; set; }


    public DeleteWithPasswordViewModel(string buttonName, string name, string handlerUrl, string onSuccess)
    {
        ButtonName = buttonName;
        Name = name;
        HandlerUrl = handlerUrl;
        OnSuccess = onSuccess;
    }
}