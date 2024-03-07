


using System.Text.Json.Serialization;
using Contracts.Models;

namespace TrainingTools.Models;

public class UserViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    public UserViewModel(User user)
    {
        Id = user.Id;
        Name = user.Name;
        Email = user.Email;
    }

    public UserViewModel(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }
    
    public UserViewModel()
    {
        
    }
}