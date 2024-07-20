using AutoMapper;
using Domain.Models;
using Domain.Models.Friendship;
using Domain.Models.TrainingPlan;
using Infrastructure.Entities;
using Infrastructure.Entities.Friendship;
using Infrastructure.Entities.TrainingPlan;

namespace Infrastructure.Mapping.Profiles;

public class InfrastructureApplicationMappingProfile : Profile
{
    public InfrastructureApplicationMappingProfile()
    {
        CreateGroupMaps();
        CreateExerciseMaps();
        CreateFriendInvitationMaps();
        CreateFriendshipMaps();
        CreateTrainingPlanMaps();
    }

    private void CreateGroupMaps()
    {
        CreateMap<Group, GroupEntity>();
        CreateMap<GroupEntity, Group>();
    }

    private void CreateExerciseMaps()
    {
        CreateMap<Exercise, ExerciseEntity>();
        CreateMap<ExerciseEntity, Exercise>();
    }
    
    private void CreateFriendInvitationMaps()
    {
        CreateMap<FriendInvitation, FriendInvitationEntity>();
        CreateMap<FriendInvitationEntity, FriendInvitation>();
    }

    private void CreateFriendshipMaps()
    {
        CreateMap<Friendship, FriendshipEntity>()
            .ForMember(dest => dest.FriendsFrom, opt => opt.MapFrom(src => src.FriendsFromDateTime));
        
        CreateMap<FriendshipEntity, Friendship>()
            .ForMember(dest => dest.FriendsFromDateTime, opt => opt.MapFrom(src => src.FriendsFrom));
    }
    
    private void CreateTrainingPlanMaps()
    {
        CreateMap<TrainingPlanEntity, TrainingPlan>()
            .ForMember(dest => dest.TrainingPlanBlocks, opt => opt.MapFrom(src => src.TrainingPlanBlocks.OrderBy(b => b.Position)));
        CreateMap<TrainingPlanBlockEntity, TrainingPlanBlock>()
            .ForMember(dest => dest.TrainingPlanBlockEntries, opt => opt.MapFrom(src => src.TrainingPlanBlockEntries.OrderBy(e => e.Position)));
        CreateMap<TrainingPlanBlockEntryEntity, TrainingPlanBlockEntry>();

        
        CreateMap<TrainingPlan, TrainingPlanEntity>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .AfterMap((src, dest) =>
            {
                for (int i = 0; i < src.TrainingPlanBlocks.Count; i++)
                    dest.TrainingPlanBlocks[i].Position = i;
            });

        CreateMap<TrainingPlanBlock, TrainingPlanBlockEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .AfterMap((src, dest) =>
            {
                for (int i = 0; i < src.TrainingPlanBlockEntries.Count; i++)
                    dest.TrainingPlanBlockEntries[i].Position = i;
            });

        CreateMap<TrainingPlanBlockEntry, TrainingPlanBlockEntryEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Group, opt => opt.Ignore())
            .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Group.Id));
    }
}