using AutoMapper;
using CommunityCar.Domain.DTOs.Dashboard.Overview;
using CommunityCar.Domain.Entities.Dashboard.widgets;

namespace CommunityCar.Infrastructure.Mappings.Dashboard.Overview;

public class WidgetProfile : Profile
{
    public WidgetProfile()
    {
        CreateMap<DashboardWidget, WidgetDto>();
        CreateMap<CreateWidgetDto, DashboardWidget>();
        CreateMap<UpdateWidgetDto, DashboardWidget>();
    }
}
