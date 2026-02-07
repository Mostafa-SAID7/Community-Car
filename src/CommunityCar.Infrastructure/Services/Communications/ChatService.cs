using AutoMapper;
using CommunityCar.Domain.DTOs.Communications;
using CommunityCar.Domain.Entities.Communications.chats;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Communications.chats;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Communications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Communications;

public class ChatService : IChatService
{
    private readonly IRepository<ChatMessage> _messageRepository;
    private readonly IRepository<ChatRoom> _chatRoomRepository;
    private readonly IRepository<ChatRoomMember> _chatRoomMemberRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IRepository<ChatMessage> messageRepository,
        IRepository<ChatRoom> chatRoomRepository,
        IRepository<ChatRoomMember> chatRoomMemberRepository,
        IRepository<ApplicationUser> userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ChatService> logger)
    {
        _messageRepository = messageRepository;
        _chatRoomRepository = chatRoomRepository;
        _chatRoomMemberRepository = chatRoomMemberRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<ChatConversationDto>> GetConversationsAsync(Guid userId)
    {
        try
        {
            var userChatRooms = await _chatRoomMemberRepository
                .GetQueryable()
                .Where(m => m.UserId == userId)
                .Include(m => m.ChatRoom)
                    .ThenInclude(cr => cr.Members)
                        .ThenInclude(m => m.User)
                .Include(m => m.ChatRoom)
                    .ThenInclude(cr => cr.Messages)
                .ToListAsync();

            var conversations = new List<ChatConversationDto>();

            foreach (var userChatRoom in userChatRooms)
            {
                var chatRoom = userChatRoom.ChatRoom;
                
                // For direct messages (non-group chats), get the other user
                if (!chatRoom.IsGroup)
                {
                    var otherMember = chatRoom.Members.FirstOrDefault(m => m.UserId != userId);
                    if (otherMember == null) continue;

                    var lastMessage = chatRoom.Messages
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault();

                    var unreadCount = chatRoom.Messages
                        .Count(m => m.SenderId != userId && 
                               m.CreatedAt > (userChatRoom.LastReadAt ?? DateTimeOffset.MinValue));

                    conversations.Add(new ChatConversationDto
                    {
                        UserId = otherMember.UserId,
                        UserName = otherMember.User.UserName ?? "Unknown",
                        ProfilePicture = otherMember.User.ProfilePictureUrl,
                        LastMessage = lastMessage?.Content,
                        LastMessageTime = lastMessage?.CreatedAt,
                        UnreadCount = unreadCount,
                        IsOnline = false // TODO: Implement online status tracking
                    });
                }
            }

            return conversations.OrderByDescending(c => c.LastMessageTime).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversations for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50)
    {
        try
        {
            var chatRoom = await GetOrCreateChatRoomAsync(userId, otherUserId);

            var messages = await _messageRepository
                .GetQueryable()
                .Where(m => m.ChatRoomId == chatRoom.Id)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var messageDtos = messages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = m.Sender.UserName ?? "Unknown",
                SenderProfilePicture = m.Sender.ProfilePictureUrl,
                ReceiverId = m.SenderId == userId ? otherUserId : userId,
                ReceiverName = string.Empty, // Will be populated by the controller if needed
                ReceiverProfilePicture = null,
                Content = m.Content,
                IsRead = m.CreatedAt <= (chatRoom.Members.FirstOrDefault(cm => cm.UserId == otherUserId)?.LastReadAt ?? DateTimeOffset.MinValue),
                CreatedAt = m.CreatedAt
            }).Reverse().ToList();

            return messageDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages between {UserId} and {OtherUserId}", userId, otherUserId);
            throw;
        }
    }

    public async Task<ChatMessageDto> SendMessageAsync(Guid senderId, SendMessageDto dto)
    {
        try
        {
            var sender = await _userRepository.GetByIdAsync(senderId);
            if (sender == null)
                throw new InvalidOperationException("Sender not found");

            var receiver = await _userRepository.GetByIdAsync(dto.ReceiverId);
            if (receiver == null)
                throw new InvalidOperationException("Receiver not found");

            var chatRoom = await GetOrCreateChatRoomAsync(senderId, dto.ReceiverId);

            var message = new ChatMessage
            {
                ChatRoomId = chatRoom.Id,
                SenderId = senderId,
                Content = dto.Content,
                Type = MessageType.Text,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _messageRepository.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            // Reload with sender info
            message = await _messageRepository
                .GetQueryable()
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == message.Id);

            return new ChatMessageDto
            {
                Id = message!.Id,
                SenderId = message.SenderId,
                SenderName = message.Sender.UserName ?? "Unknown",
                SenderProfilePicture = message.Sender.ProfilePictureUrl,
                ReceiverId = dto.ReceiverId,
                ReceiverName = receiver.UserName ?? "Unknown",
                ReceiverProfilePicture = receiver.ProfilePictureUrl,
                Content = message.Content,
                IsRead = false,
                CreatedAt = message.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {SenderId} to {ReceiverId}", senderId, dto.ReceiverId);
            throw;
        }
    }

    public async Task<bool> MarkAsReadAsync(Guid messageId, Guid userId)
    {
        try
        {
            var message = await _messageRepository
                .GetQueryable()
                .Include(m => m.ChatRoom)
                    .ThenInclude(cr => cr.Members)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null) return false;

            var member = message.ChatRoom.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return false;

            member.LastReadAt = DateTimeOffset.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
            return false;
        }
    }

    public async Task<bool> MarkConversationAsReadAsync(Guid userId, Guid otherUserId)
    {
        try
        {
            var chatRoom = await GetChatRoomAsync(userId, otherUserId);
            if (chatRoom == null) return false;

            var member = await _chatRoomMemberRepository
                .GetQueryable()
                .FirstOrDefaultAsync(m => m.ChatRoomId == chatRoom.Id && m.UserId == userId);

            if (member == null) return false;

            member.LastReadAt = DateTimeOffset.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking conversation as read for user {UserId}", userId);
            return false;
        }
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            var userChatRooms = await _chatRoomMemberRepository
                .GetQueryable()
                .Where(m => m.UserId == userId)
                .Include(m => m.ChatRoom)
                    .ThenInclude(cr => cr.Messages)
                .ToListAsync();

            var totalUnread = 0;

            foreach (var userChatRoom in userChatRooms)
            {
                var unreadCount = userChatRoom.ChatRoom.Messages
                    .Count(m => m.SenderId != userId && 
                           m.CreatedAt > (userChatRoom.LastReadAt ?? DateTimeOffset.MinValue));
                
                totalUnread += unreadCount;
            }

            return totalUnread;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
            return 0;
        }
    }

    public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
    {
        try
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            
            if (message == null || message.SenderId != userId)
                return false;

            _messageRepository.Delete(message);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            return false;
        }
    }

    private async Task<ChatRoom> GetOrCreateChatRoomAsync(Guid userId1, Guid userId2)
    {
        var chatRoom = await GetChatRoomAsync(userId1, userId2);
        
        if (chatRoom != null)
            return chatRoom;

        // Create new chat room
        chatRoom = new ChatRoom
        {
            Name = $"Chat_{userId1}_{userId2}",
            IsGroup = false,
            CreatedBy = userId1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _chatRoomRepository.AddAsync(chatRoom);
        await _unitOfWork.SaveChangesAsync();

        // Add members
        var member1 = new ChatRoomMember
        {
            ChatRoomId = chatRoom.Id,
            UserId = userId1,
            JoinedAt = DateTimeOffset.UtcNow
        };

        var member2 = new ChatRoomMember
        {
            ChatRoomId = chatRoom.Id,
            UserId = userId2,
            JoinedAt = DateTimeOffset.UtcNow
        };

        await _chatRoomMemberRepository.AddAsync(member1);
        await _chatRoomMemberRepository.AddAsync(member2);
        await _unitOfWork.SaveChangesAsync();

        return chatRoom;
    }

    private async Task<ChatRoom?> GetChatRoomAsync(Guid userId1, Guid userId2)
    {
        var chatRooms = await _chatRoomRepository
            .GetQueryable()
            .Where(cr => !cr.IsGroup)
            .Include(cr => cr.Members)
            .ToListAsync();

        return chatRooms.FirstOrDefault(cr =>
            cr.Members.Any(m => m.UserId == userId1) &&
            cr.Members.Any(m => m.UserId == userId2) &&
            cr.Members.Count == 2);
    }
}
