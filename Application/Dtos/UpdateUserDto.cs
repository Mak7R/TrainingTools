namespace Application.Dtos;

public class UpdateUserDto
{   
    public Guid UserId { get; set; }
    public string? Name { get; set; } = null;
    public bool SetPrivate { get; set; } = false;
    public bool ClearAbout { get; set; } = false;
    public IEnumerable<string>? Roles { get; set; }
}