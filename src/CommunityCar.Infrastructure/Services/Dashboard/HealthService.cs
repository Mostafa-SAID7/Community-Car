using System.Diagnostics;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Enums.Dashboard.health;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Dashboard;

public class HealthService : IHealthService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthService> _logger;
    private static readonly DateTime _startTime = DateTime.UtcNow;

    public HealthService(ApplicationDbContext context, ILogger<HealthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthStatusDto> GetHealthStatusAsync()
    {
        var database