namespace Application.Interfaces.Services;

public interface IAuthTokenService<TModel>
{
    string GenerateToken(TModel generationInfo);
}