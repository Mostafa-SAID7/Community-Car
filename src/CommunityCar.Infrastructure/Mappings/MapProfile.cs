using AutoMapper;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.maps;

namespace CommunityCar.Infrastructure.Mappings;

public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<MapPoint, MapPointDto>()
            // Location flattening
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Latitude))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.Longitude))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Location.Address))
            
            // Enum to string conversions
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            
            // Owner information
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.UserName : null))
            .ForMember(dest => dest.OwnerAvatar, opt => opt.MapFrom(src => src.Owner != null ? src.Owner.ProfilePictureUrl : null))
            
            // User context properties (set manually in service)
            .ForMember(dest => dest.IsOwner, opt => opt.Ignore())
            .ForMember(dest => dest.IsFavorited, opt => opt.Ignore())
            .ForMember(dest => dest.UserRating, opt => opt.Ignore())
            
            // Timestamp mapping
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.ModifiedAt));

        CreateMap<MapPointComment, MapPointCommentDto>()
            // User information
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User != null ? src.User.ProfilePictureUrl : null))
            
            // User context property (set manually in service)
            .ForMember(dest => dest.IsAuthor, opt => opt.Ignore())
            
            // Timestamp mapping
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.ModifiedAt));
    }
}
