using AutoMapper;
using CommunityCar.Domain.DTOs.Dashboard.Administration.Settings;
using CommunityCar.Domain.Entities.Dashboard.settings;

namespace CommunityCar.Infrastructure.Mappings.Dashboard.Administration.Settings;

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
