﻿using Contracts.Models;
using Microsoft.AspNetCore.Http;

namespace Contracts.Services;

public interface IAuthorizedUser
{
    User User { get; }
    public bool IsAuthorized { get; }
    public bool IsPasswordConfirmed { get; }
    Task<bool> Authorize(HttpContext context);
    Task Authorize(HttpContext context, string email);
    void EndAuthorization(HttpContext context);
    
    bool ConfirmPassword(string password);
    Task SaveChanges();
}