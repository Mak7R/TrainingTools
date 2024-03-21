using Contracts.Enums;
using Contracts.Models;

namespace Contracts.Services;

public interface ISelectedWorkspace
{
    IAuthorizedUser Authorized { get; }
    Workspace Workspace { get; }
    WorkspacePermission Permission { get; }
    
    Task Select(Guid workspaceId);
    void Unselect();
}