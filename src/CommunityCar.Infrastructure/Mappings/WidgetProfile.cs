using AutoMapper;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Entities.Dashboard.widgets;

namespace CommunityCar.Infrastructure.Mappings;

public class WidgetProfile : Profile
{
    public WidgetProfile()
    {
        CreateMap<DashboardWidget, WidgetDto>();
        CreateMap<CreateWidgetDto, DashboardWidget>();
        CreateMap<UpdateWidgetDto, DashboardWidget>();
    }
}
