using AutoMapper;
using Domain.Identity;
using Domain.Models;
using WebUI.Models.Account;
using WebUI.Models.Exercise;
using WebUI.Models.Group;
using WebUI.Models.User;

namespace WebUI.Mapping.Profiles;

public class UiApplicationMappingProfile : Profile
{
    public UiApplicationMappingProfile()
    {
        // ------------------------ Group Mapping ------------------------ //
        
        CreateMap<Group, GroupViewModel>();
        CreateMap<CreateGroupModel, Group>();
        CreateMap<UpdateGroupModel, Group>();
        
        // ------------------------ Exercise Mapping ------------------------ //

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
        
        // ------------------------ User Mapping ------------------------ //

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

        // ------------------------ User Mapping ------------------------ //

    }
}