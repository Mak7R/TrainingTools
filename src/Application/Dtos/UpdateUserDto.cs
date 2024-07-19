namespace Application.Dtos;

public class UpdateUserDto
{   
    public string UserName { get; set; } = string.Empty;
    public bool SetPrivate { get; set; } = false;
    public bool ClearAbout { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public bool IsTrainer { get; set; } = false;
}