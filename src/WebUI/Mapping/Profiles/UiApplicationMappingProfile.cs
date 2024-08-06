using Application.Dtos;
using AutoMapper;
using Domain.Identity;
using Domain.Models;
using Domain.Models.Friendship;
using Domain.Models.TrainingPlan;
using WebUI.Models.Account;
using WebUI.Models.Exercise;
using WebUI.Models.ExerciseResult;
using WebUI.Models.Friend;
using WebUI.Models.Group;
using WebUI.Models.TrainingPlan;
using WebUI.Models.User;

namespace WebUI.Mapping.Profiles;

public class UiApplicationMappingProfile : Profile
{
    public UiApplicationMappingProfile()
    {
        CreateGroupMaps();
        CreateExerciseMaps();
        CreateUserMaps();
        CreateExerciseResultMaps();
        CreateTrainingPlanMaps();
        CreateFriendMaps();
    }

    private void CreateGroupMaps()
    {
        CreateMap<Group, GroupViewModel>();
        CreateMap<CreateGroupModel, Group>();
        CreateMap<UpdateGroupModel, Group>();
    }

    private void CreateExerciseMaps()
    {
        CreateMap<Exercise, ExerciseViewModel>();
        CreateMap<CreateExerciseModel, Exercise>()
            .ForMember(dest => dest.Group,
                opt => opt.MapFrom(
                    src => new Group { Id = src.GroupId }));
        CreateMap<UpdateExerciseModel, Exercise>()
            .ForMember(dest => dest.Group,
                opt => opt.MapFrom(
                    src => new Group { Id = src.GroupId }));

        CreateMap<Exercise, UpdateExerciseModel>()
            .ForMember(dest => dest.GroupId,
                opt => opt.MapFrom(
                    src => src.Group.Id));
    }

    private void CreateUserMaps()
    {
        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(dest => dest.PhoneNumber,
                opt => opt.MapFrom(
                    src => src.Phone));
        CreateMap<UpdateProfileDto, ApplicationUser>()
            .ForMember(dest => dest.PhoneNumber,
                opt => opt.MapFrom(
                    src => src.Phone));
        CreateMap<CreateUserModel, ApplicationUser>()
            .ForMember(dest => dest.PhoneNumber,
                opt => opt.MapFrom(
                    src => src.Phone));


        CreateMap<ApplicationUser, UpdateProfileDto>()
            .ForMember(dest => dest.Phone,
                opt => opt.MapFrom(
                    src => src.PhoneNumber));
        CreateMap<ApplicationUser, ProfileViewModel>()
            .ForMember(dest => dest.Phone,
                opt => opt.MapFrom(
                    src => src.PhoneNumber));
        CreateMap<ApplicationUser, UserViewModel>()
            .ForMember(dest => dest.Phone,
                opt => opt.MapFrom(
                    src => src.PhoneNumber));

        CreateMap<UserInfo, UserInfoViewModel>();
    }

    private void CreateExerciseResultMaps()
    {
        CreateMap<ExerciseResult, ExerciseResultViewModel>();
        CreateMap<Approach, ApproachViewModel>();
    }

    private void CreateTrainingPlanMaps()
    {
        CreateMap<TrainingPlan, TrainingPlanViewModel>();
        CreateMap<TrainingPlanBlock, TrainingPlanBlockViewModel>();
        CreateMap<TrainingPlanBlockEntry, TrainingPlanBlockEntryViewModel>();

        CreateMap<TrainingPlan, UpdateTrainingPlanModel>()
            .ForMember(dest => dest.NewTitle, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.UserName))
            .ForMember(dest => dest.Blocks, opt => opt.MapFrom(src => src.TrainingPlanBlocks));

        CreateMap<TrainingPlanBlock, UpdateTrainingPlanBlockModel>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Entries, opt => opt.MapFrom(src => src.TrainingPlanBlockEntries));
        CreateMap<TrainingPlanBlockEntry, UpdateTrainingPlanBlockEntryModel>()
            .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Group.Id));
    }

    private void CreateFriendMaps()
    {
        CreateMap<FriendInvitation, FriendInvitationViewModel>();
        CreateMap<(IEnumerable<ApplicationUser> Friends, IEnumerable<FriendInvitation> InvitationsFor,
                IEnumerable<FriendInvitation> InvitationsOf), FriendRelationshipsInfoViewModel>()
            .ForMember(dest => dest.Friends, opt => opt.MapFrom(src => src.Friends))
            .ForMember(dest => dest.InvitationsFor, opt => opt.MapFrom(src => src.InvitationsFor))
            .ForMember(dest => dest.InvitationsOf, opt => opt.MapFrom(src => src.InvitationsOf));
    }
}