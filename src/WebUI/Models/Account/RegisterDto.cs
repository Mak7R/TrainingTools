﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using static Domain.Rules.DataSizes.ApplicationUserDataSizes;

namespace WebUI.Models.Account;

public class RegisterDto
{
    [Display(Name = "Nickname")]
    [Required(ErrorMessage = "Nickname is required")]
    [StringLength(MaxUsernameSize, MinimumLength = MinUsernameSize,
        ErrorMessage = "Nickname length must have less than 64 characters")]
    [Remote("IsUserNameFree", "Account", ErrorMessage = "This username already registered")]
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email is invalid")]
    [StringLength(MaxEmailSize, ErrorMessage = "Email must be shorter")]
    [Remote("IsEmailFree", "Account", ErrorMessage = "This email already registered")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone number")]
    [Phone(ErrorMessage = "Phone is invalid")]
    public string? Phone { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Confirm password")]
    [Required(ErrorMessage = "Confirm password is required")]
    [Compare(nameof(Password), ErrorMessage = "Confirm password must be same with password")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = false;
}