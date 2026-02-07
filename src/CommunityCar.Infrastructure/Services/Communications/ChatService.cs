using AutoMapper;
using CommunityCar.Domain.DTOs.Communications;
using CommunityCar.Domain.Entities.Communications.chats;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Communications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Communications;

public class ChatService : IChatService
{
    private readonly IRepository<ChatMessage> _messageRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _