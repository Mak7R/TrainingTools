using AutoMapper;
using Domain.Models;
using Infrastructure.Entities;

namespace Infrastructure.Mapping.Profiles;

public class InfrastructureApplicationMappingProfile : Profile
{
    public InfrastructureApplicationMappingProfile()
    {
        CreateGroupMaps();
    }

    private void CreateGroupMaps()
    {
        CreateMap<Group, GroupEntity>();
        CreateMap<GroupEntity, Group>();
    }
}