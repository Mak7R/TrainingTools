using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IWorkspacesService : IAuthorizeService
{
    Task AddAsync(Workspace workspace);
    Task<Workspace?> GetWorkspaceAsync(Expression<Func<Workspace, bool>> expression);
    Task<IEnumerable<Workspace>> GetWorkspacesAsync();
    Task UpdateAsync(Guid workspaceId, UpdateWorkspaceModel workspaceModel);
    Task RemoveAsync(Workspace workspace);
    
    public record UpdateWorkspaceModel(string Name);
}