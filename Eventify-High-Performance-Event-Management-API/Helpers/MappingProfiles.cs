using AutoMapper;
using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Models;

namespace Eventify_High_Performance_Event_Management_API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<UserToAddDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); 
            CreateMap<User, UserToReturnDto>(); 
        }
    }
}