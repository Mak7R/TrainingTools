using Contracts.Enums;

namespace Contracts.Extensions;

public static class WorkspacePermissionExtension
{
    public static bool HasPreviewPermission(this WorkspacePermission permission)
    {
        switch (permission)
        {
            case WorkspacePermission.Unauthorized:
            case WorkspacePermission.PermissionDenied:
                return false;
            case WorkspacePermission.FollowedPermission:
            case WorkspacePermission.ViewPermission:
            case WorkspacePermission.UsePermission:
            case WorkspacePermission.EditPermission:
            case WorkspacePermission.OwnerPermission:
                return true;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
        }
    }
    public static bool HasViewPermission(this WorkspacePermission permission)
    {
        switch (permission)
        {
            case WorkspacePermission.Unauthorized:
            case WorkspacePermission.PermissionDenied:
            case WorkspacePermission.FollowedPermission:
                return false;
            case WorkspacePermission.ViewPermission:
            case WorkspacePermission.UsePermission:
            case WorkspacePermission.EditPermission:
            case WorkspacePermission.OwnerPermission:
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
        }
    }
    
    public static bool HasUsePermission(this WorkspacePermission permission)
    {
        switch (permission)
        {
            case WorkspacePermission.Unauthorized:
            case WorkspacePermission.PermissionDenied:
            case WorkspacePermission.FollowedPermission:
            case WorkspacePermission.ViewPermission:
                return false;
            case WorkspacePermission.UsePermission:
            case WorkspacePermission.EditPermission:
            case WorkspacePermission.OwnerPermission:
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
        }
    }
    
    public static bool HasEditPermission(this WorkspacePermission permission)
    {
        switch (permission)
        {
            case WorkspacePermission.Unauthorized:
            case WorkspacePermission.PermissionDenied:
            case WorkspacePermission.FollowedPermission:
            case WorkspacePermission.ViewPermission:
            case WorkspacePermission.UsePermission:
                return false;
            case WorkspacePermission.EditPermission:
            case WorkspacePermission.OwnerPermission:
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
        }
    }   
    
    public static bool HasOwnerPermission(this WorkspacePermission permission)
    {
        switch (permission)
        {
            case WorkspacePermission.Unauthorized:
            case WorkspacePermission.PermissionDenied:
            case WorkspacePermission.FollowedPermission:
            case WorkspacePermission.ViewPermission:
            case WorkspacePermission.UsePermission:
            case WorkspacePermission.EditPermission:
                return false;
            case WorkspacePermission.OwnerPermission:
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
        }
    }
}