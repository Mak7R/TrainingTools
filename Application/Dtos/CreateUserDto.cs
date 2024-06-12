using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using static Domain.Rules.DataSizes.ApplicationUser;

namespace Application.Dtos;

public class CreateUserDto
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
    public bool IsPublic { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
}