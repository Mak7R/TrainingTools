using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces.Services;
using Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Application.Services;

public class JwtService : IAuthTokenService<TokenGenerationInfo>
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GenerateToken(TokenGenerationInfo generationInfo)
    {
        var claims = new List<Claim> { 
            new (JwtRegisteredClaimNames.Sub, generationInfo.User.Id.ToString()),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), 
            new (JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new (ClaimTypes.NameIdentifier, generationInfo.User.Id.ToString()), 
            new (ClaimTypes.Name, generationInfo.User.UserName ?? string.Empty),
            new (ClaimTypes.Email, generationInfo.User.Email ?? string.Empty)
        };
        claims.AddRange(generationInfo.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var key = _configuration["Jwt:SecretKey"] ?? throw new NullReferenceException("Jwt:SecretKey can't be null");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));
        
        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: expiration,
            signingCredentials: signingCredentials
       );
       
        var tokenHandler = new JwtSecurityTokenHandler();
       
        return tokenHandler.WriteToken(token);
    }
}