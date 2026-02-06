using AutoMapper;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Entities.Community.friends;
using CommunityCar.Domain.DTOs.Identity;

namespace CommunityCar.Infrastructure.Mappings;

/// <summary>
/// AutoMapper profile for Identity module mappings
/// </summary>
public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        // User to DTOs
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<ApplicationUser, UserSearchDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        // ViewModels to Commands (if needed) - Keeping these if used by MediatR commands in Domain that Infra sees? 
        // Actually Infrastructure might not see Web ViewModels if we want strict layering. 
        // BUT for now resolving compilation is key.
        // If I remove Web reference from Infra, I break these mappings if they use ViewModels.
        // The goal is to let Domain compile. Infrastructure can reference Web? NO. Infrastructure -> Domain. Web -> Infrastructure.
        // Infrastructure CANNOT reference Web.
        // So I MUST remove usages of ViewModels in Infrastructure.
        
        // Removed ViewModel mappings. Controller will map ViewModel -> Command/DTO.
    }
}
