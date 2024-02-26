


using Contracts.Models;

namespace TrainingTools.Models;

public class UserViewModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public UserViewModel(User user)
    {
        Id = user.Id;
        Name = user.Name;
        Email = user.Email;
    }
}