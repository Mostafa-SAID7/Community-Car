using AutoMapper;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Entities.Dashboard.security;

namespace CommunityCar.Infrastructure.Mappings;

public class AuditLogProfile : Profile
{
    public AuditLogProfile()
    {
        CreateMap<AuditLog, AuditLogDto>();
    }
}
