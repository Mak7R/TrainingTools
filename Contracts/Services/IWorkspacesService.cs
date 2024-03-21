using System.Linq.Expressions;
using Contracts.Models;

namespace Contracts.Services;

public interface IWorkspacesService
{
    Task Add(Workspace workspace);
    Task<Workspace?> Get(Guid workspaceId);
    Task<IEnumerable<Workspace>> GetRange(Expression<Func<Workspace, bool>> expression);
    Task Update(Action<Workspace> updater);
    Task Remove(Guid workspaceId);
}