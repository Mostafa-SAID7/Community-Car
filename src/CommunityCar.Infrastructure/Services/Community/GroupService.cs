using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.groups;
using CommunityCar.Domain.Enums.Community.groups;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class GroupService : IGroupService
{
    private readonly IRepository<CommunityGroup> _groupRepository;
    private readonly IRepository<GroupMember> _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GroupService> _logger;

    public GroupService(
        IRepository<CommunityGroup> groupRepository,
        IRepository<GroupMember> memberRepository,
        IUnitOfWork unitOfWork,
        ILogger<GroupService> logger)
    {
        _groupRepository = groupRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<GroupDto>> CreateGroupAsync(string name, string description, Guid creatorId, bool isPrivate)
    {
        try
        {
            var group = new CommunityGroup(name, description, creatorId, isPrivate);
            await _groupRepository.AddAsync(group);
            
            // Add creator as admin member
            var member = new GroupMember(group.Id, creatorId, GroupMemberRole.Admin);
            await _memberRepository.AddAsync(member);
            
            await _unitOfWork.SaveChangesAsync();

            var dto = await GetGroupByIdAsync(group.Id, creatorId);
            return Result.Success<GroupDto>(dto!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return Result.Failure<GroupDto>("Failed to create group");
        }
    }

    public async Task<Result<GroupDto>> UpdateGroupAsync(Guid groupId, string name, string description, bool isPrivate, Guid userId)
    {
        try
        {
            var group = await _groupRepository
                .GetByIdAsync(groupId);

            if (group == null)
                return Result.Failure<GroupDto>("Group not found");

            var userRole = await GetUserRoleAsync(groupId, userId);
            if (userRole != GroupMemberRole.Admin && group.CreatorId != userId)
                return Result.Failure<GroupDto>("You don't have permission to update this group");

            group.Update(name, description, isPrivate);
            await _unitOfWork.SaveChangesAsync();

            var dto = await GetGroupByIdAsync(groupId, userId);
            return Result.Success<GroupDto>(dto!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", groupId);
            return Result.Failure<GroupDto>("Failed to update group");
        }
    }

    public async Task<Result> DeleteGroupAsync(Guid groupId, Guid userId)
    {
        try
        {
            var group = await _groupRepository
                .GetByIdAsync(groupId);

            if (group == null)
                return Result.Failure("Group not found");

            if (group.CreatorId != userId)
                return Result.Failure("Only the group creator can delete the group");

            _groupRepository.Delete(group);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId}", groupId);
            return Result.Failure("Failed to delete group");
        }
    }

    public async Task<GroupDto?> GetGroupByIdAsync(Guid groupId, Guid? currentUserId = null)
    {
        var group = await _groupRepository
            .GetQueryable()
            .Include(g => g.Creator)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return null;

        return await MapToDto(group, currentUserId);
    }

    public async Task<GroupDto?> GetGroupBySlugAsync(string slug, Guid? currentUserId = null)
    {
        var group = await _groupRepository
            .GetQueryable()
            .Include(g => g.Creator)
            .FirstOrDefaultAsync(g => g.Slug == slug);

        if (group == null)
            return null;

        return await MapToDto(group, currentUserId);
    }

    public async Task<PagedResult<GroupDto>> GetGroupsAsync(QueryParameters parameters, Guid? currentUserId = null)
    {
        var query = _groupRepository
            .GetQueryable()
            .Include(g => g.Creator)
            .Where(g => !g.IsPrivate)
            .OrderByDescending(g => g.LastActivityAt);

        var totalCount = await query.CountAsync();
        var groups = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = new List<GroupDto>();
        foreach (var group in groups)
        {
            dtos.Add(await MapToDto(group, currentUserId));
        }

        return new PagedResult<GroupDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<GroupDto>> GetUserGroupsAsync(Guid userId, QueryParameters parameters)
    {
        var query = _memberRepository
            .GetQueryable()
            .Include(gm => gm.Group)
            .ThenInclude(g => g.Creator)
            .Where(gm => gm.UserId == userId)
            .Select(gm => gm.Group)
            .OrderByDescending(g => g.LastActivityAt);

        var totalCount = await query.CountAsync();
        var groups = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = new List<GroupDto>();
        foreach (var group in groups)
        {
            dtos.Add(await MapToDto(group, userId));
        }

        return new PagedResult<GroupDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<GroupDto>> SearchGroupsAsync(string query, QueryParameters parameters, Guid? currentUserId = null)
    {
        var groupQuery = _groupRepository
            .GetQueryable()
            .Include(g => g.Creator)
            .Where(g => !g.IsPrivate && (g.Name.Contains(query) || g.Description.Contains(query)))
            .OrderByDescending(g => g.LastActivityAt);

        var totalCount = await groupQuery.CountAsync();
        var groups = await groupQuery
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = new List<GroupDto>();
        foreach (var group in groups)
        {
            dtos.Add(await MapToDto(group, currentUserId));
        }

        return new PagedResult<GroupDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<Result> JoinGroupAsync(Guid groupId, Guid userId)
    {
        try
        {
            var group = await _groupRepository
                .GetByIdAsync(groupId);

            if (group == null)
                return Result.Failure("Group not found");

            var isMember = await IsUserMemberAsync(groupId, userId);
            if (isMember)
                return Result.Failure("You are already a member of this group");

            var member = new GroupMember(groupId, userId);
            await _memberRepository.AddAsync(member);
            
            group.IncrementMemberCount();
            group.UpdateActivity();
            
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group {GroupId}", groupId);
            return Result.Failure("Failed to join group");
        }
    }

    public async Task<Result> LeaveGroupAsync(Guid groupId, Guid userId)
    {
        try
        {
            var group = await _groupRepository
                .GetByIdAsync(groupId);

            if (group == null)
                return Result.Failure("Group not found");

            if (group.CreatorId == userId)
                return Result.Failure("Group creator cannot leave the group");

            var member = await _memberRepository
                .GetQueryable()
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (member == null)
                return Result.Failure("You are not a member of this group");

            _memberRepository.Delete(member);
            group.DecrementMemberCount();
            
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group {GroupId}", groupId);
            return Result.Failure("Failed to leave group");
        }
    }

    public async Task<Result> RemoveMemberAsync(Guid groupId, Guid memberId, Guid userId)
    {
        try
        {
            var group = await _groupRepository
                .GetByIdAsync(groupId);

            if (group == null)
                return Result.Failure("Group not found");

            var userRole = await GetUserRoleAsync(groupId, userId);
            if (userRole != GroupMemberRole.Admin && group.CreatorId != userId)
                return Result.Failure("You don't have permission to remove members");

            var member = await _memberRepository
                .GetQueryable()
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == memberId);

            if (member == null)
                return Result.Failure("Member not found");

            if (member.UserId == group.CreatorId)
                return Result.Failure("Cannot remove the group creator");

            _memberRepository.Delete(member);
            group.DecrementMemberCount();
            
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from group {GroupId}", groupId);
            return Result.Failure("Failed to remove member");
        }
    }

    public async Task<Result> UpdateMemberRoleAsync(Guid groupId, Guid memberId, GroupMemberRole role, Guid userId)
    {
        try
        {
            var group = await _groupRepository
                .GetByIdAsync(groupId);

            if (group == null)
                return Result.Failure("Group not found");

            if (group.CreatorId != userId)
                return Result.Failure("Only the group creator can change member roles");

            var member = await _memberRepository
                .GetQueryable()
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == memberId);

            if (member == null)
                return Result.Failure("Member not found");

            if (member.UserId == group.CreatorId)
                return Result.Failure("Cannot change the creator's role");

            switch (role)
            {
                case GroupMemberRole.Admin:
                    member.PromoteToAdmin();
                    break;
                case GroupMemberRole.Moderator:
                    member.PromoteToModerator();
                    break;
                case GroupMemberRole.Member:
                    member.DemoteToMember();
                    break;
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member role in group {GroupId}", groupId);
            return Result.Failure("Failed to update member role");
        }
    }

    public async Task<PagedResult<GroupMemberDto>> GetGroupMembersAsync(Guid groupId, QueryParameters parameters)
    {
        var query = _memberRepository
            .GetQueryable()
            .Include(gm => gm.User)
            .Where(gm => gm.GroupId == groupId)
            .OrderByDescending(gm => gm.Role)
            .ThenBy(gm => gm.JoinedAt);

        var totalCount = await query.CountAsync();
        var members = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = members.Select(m => new GroupMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            UserName = $"{m.User?.FirstName ?? "Unknown"} {m.User?.LastName ?? "User"}",
            ProfilePictureUrl = m.User?.ProfilePictureUrl,
            Slug = m.User?.Slug,
            Role = m.Role,
            JoinedAt = m.JoinedAt
        }).ToList();

        return new PagedResult<GroupMemberDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<bool> IsUserMemberAsync(Guid groupId, Guid userId)
    {
        return await _memberRepository
            .GetQueryable()
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
    }

    public async Task<GroupMemberRole?> GetUserRoleAsync(Guid groupId, Guid userId)
    {
        var member = await _memberRepository
            .GetQueryable()
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        return member?.Role;
    }

    private async Task<GroupDto> MapToDto(CommunityGroup group, Guid? currentUserId)
    {
        var dto = new GroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            Slug = group.Slug,
            IsPrivate = group.IsPrivate,
            CreatorId = group.CreatorId,
            CreatorName = $"{group.Creator?.FirstName ?? "Unknown"} {group.Creator?.LastName ?? "User"}",
            ImageUrl = group.ImageUrl,
            MemberCount = group.MemberCount,
            LastActivityAt = group.LastActivityAt,
            CreatedAt = group.CreatedAt,
            IsUserMember = false,
            UserRole = null
        };

        if (currentUserId.HasValue)
        {
            dto.IsUserMember = await IsUserMemberAsync(group.Id, currentUserId.Value);
            dto.UserRole = await GetUserRoleAsync(group.Id, currentUserId.Value);
        }

        return dto;
    }
}


