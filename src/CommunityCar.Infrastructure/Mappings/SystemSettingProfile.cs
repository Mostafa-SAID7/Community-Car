using AutoMapper;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Entities.Dashboard.settings;

namespace CommunityCar.Infrastructure.Mappings;

public class SystemSettingProfile : Profile
{
    public SystemSettingProfile()
    {
        CreateMap<SystemSetting, SystemSettingDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.ToString()));

        CreateMap<CreateSystemSettingDto, SystemSetting>();
        CreateMap<UpdateSystemSettingDto, SystemSetting>();
    }
}
