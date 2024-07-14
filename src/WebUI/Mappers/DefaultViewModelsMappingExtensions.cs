using Application.Dtos;
using Application.Models;
using Domain.Identity;
using Domain.Models;
using Domain.Models.TrainingPlan;
using WebUI.Models.AccountModels;
using WebUI.Models.ExerciseModels;
using WebUI.Models.ExerciseResultModels;
using WebUI.Models.FriendModels;
using WebUI.Models.GroupModels;
using WebUI.Models.TrainingPlanModels;
using WebUI.Models.UserModels;

namespace WebUI.Mappers;

public static class DefaultViewModelsMappingExtensions
{
    public static ProfileViewModel ToProfileViewModel(this ApplicationUser applicationUser, IEnumerable<string> roles)
    {
        return new ProfileViewModel
        {
            Username = applicationUser.UserName,
            About = applicationUser.About,
            Email = applicationUser.Email,
            Phone = applicationUser.PhoneNumber,
            Roles = roles,
            IsPublic = applicationUser.IsPublic
        };
    }
    
    public static UserViewModel ToUserViewModel(this ApplicationUser applicationUser)
    {
        return new UserViewModel
        {
            Username = applicationUser.UserName,
            Email = applicationUser.Email,
            Phone = applicationUser.PhoneNumber,
            About = applicationUser.About,
            IsPublic = applicationUser.IsPublic
        };
    }

    public static UserInfoViewModel ToUserInfoViewModel(this UserInfo userInfo)
    {
        return new UserInfoViewModel
        {
            User = userInfo.User.ToUserViewModel(),
            RelationshipState = userInfo.RelationshipState,
            Roles = userInfo.Roles
        };
    }

    public static GroupViewModel ToGroupViewModel(this Group group)
    {
        return new GroupViewModel
        {
            Id = group.Id,
            Name = group.Name
        };
    }

    public static ExerciseViewModel ToExerciseViewMode(this Exercise exercise)
    {
        return new ExerciseViewModel
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Group = exercise.Group.ToGroupViewModel(),
            About = exercise.About
        };
    }

    public static FriendInvitationViewModel ToFriendInvitationViewModel(this FriendInvitation friendInvitation)
    {
        return new FriendInvitationViewModel
        {
            Invitor = friendInvitation.Invitor.ToUserViewModel(),
            Target = friendInvitation.Target.ToUserViewModel(),
            InvitationTime = friendInvitation.InvitationTime
        };
    }

    public static FriendRelationshipsInfoViewModel ToFriendRelationshipsInfoViewModel(this IEnumerable<ApplicationUser> friends,
        IEnumerable<FriendInvitation> invitationsFor, IEnumerable<FriendInvitation> invitationsOf)
    {
        return new FriendRelationshipsInfoViewModel
        {
            InvitationsFor = invitationsFor.Select(i => i.ToFriendInvitationViewModel()),
            Friends = friends.Select(f => f.ToUserViewModel()),
            InvitationsOf = invitationsOf.Select(i => i.ToFriendInvitationViewModel())
        };
    }

    public static ExerciseResultViewModel ToExerciseResultViewModel(this ExerciseResult exerciseResult)
    {
        return new ExerciseResultViewModel
        {
            Owner = exerciseResult.Owner.ToUserViewModel(),
            Exercise = exerciseResult.Exercise.ToExerciseViewMode(),
            ApproachInfos = exerciseResult.ApproachInfos
                .Select(ai => new ApproachViewModel(ai.Weight, ai.Count, ai.Comment)).ToList()
        };
    }

    public static TrainingPlanViewModel ToTrainingPlanViewModel(this TrainingPlan trainingPlan)
    {
        return new TrainingPlanViewModel
        {
            Title = trainingPlan.Title,
            Author = trainingPlan.Author.ToUserViewModel(),
            IsPublic = trainingPlan.IsPublic,
            TrainingPlanBlocks = trainingPlan.TrainingPlanBlocks
                .Select(b => b.ToTrainingPlanBlockViewModel())
                .ToList()
        };
    }

    public static TrainingPlanBlockViewModel ToTrainingPlanBlockViewModel(this TrainingPlanBlock block)
    {
        return new TrainingPlanBlockViewModel
        {
            Name = block.Name,
            TrainingPlanBlockEntries = block.TrainingPlanBlockEntries
                .Select(e => e.ToTrainingPlanBlockEntryViewModel())
                .ToList()
        };
    }
    
    public static TrainingPlanBlockEntryViewModel ToTrainingPlanBlockEntryViewModel(this TrainingPlanBlockEntry entry)
    {
        return new TrainingPlanBlockEntryViewModel
        {
            Group = entry.Group.ToGroupViewModel(),
            Description = entry.Description
        };
    }
}