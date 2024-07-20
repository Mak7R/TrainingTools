using AutoMapper;
using Domain.Models;
using Domain.Models.TrainingPlan;
using Infrastructure.Entities;
using Infrastructure.Entities.TrainingPlanEntities;

namespace Infrastructure.Mapping.Profiles;

public class InfrastructureApplicationMappingProfile : Profile
{
    public InfrastructureApplicationMappingProfile()
    {
        CreateGroupMaps();
        CreateTrainingPlanMaps();
    }

    private void CreateGroupMaps()
    {
        CreateMap<Group, GroupEntity>();
        CreateMap<GroupEntity, Group>();
    }

    private void CreateTrainingPlanMaps()
    {
        CreateMap<TrainingPlanEntity, TrainingPlan>()
            .ForMember(dest => dest.TrainingPlanBlocks, opt => opt.MapFrom(src => src.TrainingPlanBlocks.OrderBy(b => b.Position)));
        CreateMap<TrainingPlanBlockEntity, TrainingPlanBlock>()
            .ForMember(dest => dest.TrainingPlanBlockEntries, opt => opt.MapFrom(src => src.TrainingPlanBlockEntries.OrderBy(e => e.Position)));
        CreateMap<TrainingPlanBlockEntryEntity, TrainingPlanBlockEntry>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Desctiption));

        
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
            .ForMember(dest => dest.Desctiption, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.Group.Id));
    }
}