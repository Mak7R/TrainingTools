namespace Application.Dtos;

public class UpdateUserDto
{   
    public Guid UserId { get; set; }
    public string? Username { get; set; } = null;
    public bool SetPrivate { get; set; } = false;
    public bool ClearAbout { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public bool IsTrainer { get; set; } = false;
}