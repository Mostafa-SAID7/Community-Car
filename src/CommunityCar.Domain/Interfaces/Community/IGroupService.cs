using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Enums.Community.groups;

namespace CommunityCar.Domain.Interfaces.Community;

public interface IGroupService
{
    Task<Result<GroupDto>> CreateGroupAsync(string name, string description, Guid creatorId, bool isPrivate);
    Task<Result<GroupDto>> UpdateGroupAsync(Guid groupId, string name, string description, bool isPrivate, Guid userId);
    Task<Result> DeleteGroupAsync(Guid groupId, Guid userId);
    Task<GroupDto?> GetGroupByIdAsync(Guid groupId, Guid? currentUserId = null);
    Task<GroupDto?> GetGroupBySlugAsync(string slug, Guid? currentUserId = null);
    Task<PagedResult<GroupDto>> GetGroupsAsync(QueryParameters parameters, Guid? currentUserId = null);
    Task<PagedResult<GroupDto>> GetUserGroupsAsync(Guid userId, QueryParameters parameters);
    Task<PagedResult<GroupDto>> SearchGroupsAsync(string query, QueryParameters parameters, Guid? currentUserId = null);
    
    Task<Result> JoinGroupAsync(Guid groupId, Guid userId);
    Task<Result> LeaveGroupAsync(Guid groupId, Guid userId);
    Task<Result> RemoveMemberAsync(Guid groupId, Guid memberId, Guid userId);
    Task<Result> UpdateMemberRoleAsync(Guid groupId, Guid memberId, GroupMemberRole role, Guid userId);
    
    Task<PagedResult<GroupMemberDto>> GetGroupMembersAsync(Guid groupId, QueryParameters parameters);
    Task<bool> IsUserMemberAsync(Guid groupId, Guid userId);
    Task<GroupMemberRole?> GetUserRoleAsync(Guid groupId, Guid userId);
}
