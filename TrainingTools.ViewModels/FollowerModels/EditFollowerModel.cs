using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Contracts.Enums;
using Contracts.Models;

namespace TrainingTools.ViewModels;

public class EditFollowerModel
{
    [Required]
    [JsonPropertyName("rights")]
    public FollowerRights Rights { get; set; }
}