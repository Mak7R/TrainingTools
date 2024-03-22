using System.Text.Json.Serialization;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class UserViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [JsonPropertyName("follows")]
    public List<FollowViewModel> Follows { get; set; }
    
    public UserViewModel(Guid id, string name, string email, List<FollowViewModel> follows)
    {
        Id = id;
        Name = name;
        Email = email;
        Follows = follows;
    }
    
    public UserViewModel()
    {
        
    }
}