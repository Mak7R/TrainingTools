using System.Text.Json;
using Contracts.Enums;
using Contracts.Models;
using TrainingTools.ViewModels;

namespace Services;

public static class ViewModelsCreatorExtensions
{
    public static FollowViewModel ToFollowViewModel(this FollowerRelationship relationship)
    {
        return new FollowViewModel(relationship.WorkspaceId, relationship.Workspace.Name);
    } 
    public static FollowerViewModel ToFollowerViewModel(this FollowerRelationship relationship)
    {
        return new FollowerViewModel(relationship.Follower.ToPublicUserViewModel(), relationship.FollowerRights);
    }
    public static PublicUserViewModel ToPublicUserViewModel(this User user)
    {
        return new PublicUserViewModel(user.Id, user.Name, user.Email);
    }

    public static PublicWorkspaceViewModel ToPublicWorkspaceViewModel(this Workspace workspace,
        WorkspacePermission permission)
    {
        return new PublicWorkspaceViewModel(workspace.Id, workspace.Name, workspace.Owner.ToPublicUserViewModel(), permission);
    }
    
    public static FullWorkspaceViewModel ToFullWorkspaceViewModel(this Workspace workspace, IEnumerable<Group> groups,
        IEnumerable<Exercise> exercises, WorkspacePermission permission)
    {
        return new FullWorkspaceViewModel(
            workspace.Id,
            workspace.Name,
            workspace.Owner.ToPublicUserViewModel(),
            groups.Select(g => g.ToGroupViewModel(permission)),
            exercises.Select(e => e.ToExerciseViewModel(permission)), permission
            );
    }

    public static FullExerciseViewModel ToFullExerciseViewModel(this Exercise exercise, ExerciseResults? results, IEnumerable<ExerciseResults> allResults, WorkspacePermission permission)
    {
        return new FullExerciseViewModel(
            exercise.Id, 
            exercise.Name, 
            exercise.Workspace.ToWorkspaceViewModel(permission),
            exercise.Group?.ToGroupViewModel(permission), 
            results?.ToExerciseResultsViewModel(),
            allResults.Select(r => r.ToExerciseResultsViewModel()));
    }

    public static ExerciseResultsViewModel ToExerciseResultsViewModel(this ExerciseResults results)
    {
        return new ExerciseResultsViewModel(results.Id, results.Owner.ToPublicUserViewModel(),
            JsonSerializer.Deserialize<ExerciseResultsObjectViewModel>(results.ResultsJson) ?? []); // Exception if Json is not valid or empty
    }
    
    public static GroupViewModel ToGroupViewModel(this Group group, WorkspacePermission permission)
    {
        return new GroupViewModel(group.Id, group.Name, group.Workspace.ToWorkspaceViewModel(permission));
    }
    
    public static ExerciseViewModel ToExerciseViewModel(this Exercise exercise, WorkspacePermission permission)
    {
        return new ExerciseViewModel(exercise.Id, exercise.Name, exercise.Workspace.ToWorkspaceViewModel(permission), exercise.Group?.ToGroupViewModel(permission));
    }
    
    public static WorkspaceViewModel ToWorkspaceViewModel(this Workspace workspace, WorkspacePermission permission)
    {
        return new WorkspaceViewModel(workspace.Id, workspace.Name, workspace.Owner.ToPublicUserViewModel(), workspace.IsPublic, permission);
    }

    public static UserViewModel ToUserViewModel(this User user)
    {
        return new UserViewModel(user.Id, user.Name, user.Email, user.Follows.Select(fr => fr.ToFollowViewModel()).ToList());
    }
}