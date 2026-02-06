using AutoMapper;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Entities.Dashboard.security;

namespace CommunityCar.Infrastructure.Mappings;

public class SecurityAlertProfile : Profile
{
    public SecurityAlertProfile()
    {
        CreateMap<SecurityAlert, SecurityAlertDto>()
            .ForMember(dest => dest.SeverityText, opt => opt.MapFrom(src => src.Severity.ToString()))
            .ForMember(dest => dest.AlertTypeText, opt => opt.MapFrom(src => src.AlertType.ToString()));

        CreateMap<CreateSecurityAlertDto, SecurityAlert>()
            .ConstructUsing(dto => new SecurityAlert(
                dto.Title,
                dto.Severity,
                dto.AlertType,
                dto.Description,
                dto.Source,
                dto.IpAddress,
                dto.UserAgent,
                dto.AffectedUserId,
                dto.AffectedUserName));
    }
}
