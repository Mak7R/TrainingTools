using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IWorkspacesService : IAuthorizeService
{
    Task Add(Workspace workspace);
    Task<Workspace?> Get(Expression<Func<Workspace, bool>> expression);
    Task<IEnumerable<Workspace>> GetAll();
    Task Update(Guid workspaceId, Action<Workspace> updater);
    Task Remove(Workspace workspace);
}