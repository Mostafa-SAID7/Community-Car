using AutoMapper;
using CommunityCar.Domain.DTOs.Dashboard.Monitoring.Audit;
using CommunityCar.Domain.Entities.Dashboard.security;

namespace CommunityCar.Infrastructure.Mappings.Dashboard.Monitoring.Audit;

public class AuditLogProfile : Profile
{
    public AuditLogProfile()
    {
        CreateMap<AuditLog, AuditLogDto>();
    }
}
