using AutoMapper;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Entities.Dashboard.KPIs;

namespace CommunityCar.Infrastructure.Mappings;

public class KPIProfile : Profile
{
    public KPIProfile()
    {
        CreateMap<KPI, KPIDto>()
            .ForMember(dest => dest.Trend, opt => opt.MapFrom(src => src.GetTrend()))
            .ForMember(dest => dest.TrendDirection, opt => opt.MapFrom(src => src.GetTrend()))
            .ForMember(dest => dest.ChangePercentage, opt => opt.MapFrom(src => 
                src.PreviousValue.HasValue && src.PreviousValue.Value != 0 
                    ? src.GetChangePercentage() 
                    : (double?)null))
            .ForMember(dest => dest.TargetAchievementPercentage, opt => opt.MapFrom(src =>
                src.Target.HasValue && src.Target.Value > 0
                    ? (src.Value / src.Target.Value) * 100
                    : (double?)null))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<CreateKPIDto, KPI>()
            .ConstructUsing(dto => new KPI(dto.Name, dto.Code, dto.Value, dto.Unit, dto.Category, dto.Description));
    }
}
