using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public MessageRepository(DataContext context, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;
    }

    public Message AddMessage(CreateMessageDto createMessageDto)
    {
        var message = new Message()
        {
            SenderId = createMessageDto.Sender.Id,
            RecipientId = createMessageDto.Recipient.Id,
            SenderUsername = createMessageDto.Sender.UserName,
            RecipientUsername = createMessageDto.Recipient.UserName,
            Content = createMessageDto.Content
        };
        
        _context.Messages.Add(message);

        return message;
    }

    public void DeleteMessage(Message message, string username)
    {
        if(message.SenderUsername == username) message.SenderDeleted = true;

        if(message.RecipientUsername == username) message.RecipientDeleted = true;

        if (message.SenderDeleted && message.RecipientDeleted)
        {
            _context.Messages.Remove(message);
        }
    }

    public async Task<Message> GetMessageAsync(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDto>> GetMessageForUserAsync(MessageParams messageParams)
    {
        IQueryable<Message> messages = _context.Messages.OrderByDescending(m => m.MessageSentAt);

        messages = messageParams.Container switch
        {
            "Inbox" => messages.Where(m => m.Recipient.UserName == messageParams.Username && !m.RecipientDeleted),
            "Outbox" => messages.Where(m => m.Sender.UserName == messageParams.Username && !m.SenderDeleted),
            _ => messages.Where(m => m.Recipient.UserName == messageParams.Username && m.DateRead == null)
        };

        var messagesDto = messages.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

        return await PagedList<MessageDto>.CreateAsync(messagesDto, messageParams.PageIndex, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender).ThenInclude(u => u.Photos)
            .Include(m => m.Recipient).ThenInclude(u => u.Photos)
            .Where(m => m.SenderUsername == currentUsername && m.RecipientUsername == recipientUsername && !m.SenderDeleted
                || m.SenderUsername == recipientUsername && m.RecipientUsername == currentUsername && !m.RecipientDeleted)
            .OrderBy(m => m.MessageSentAt).ToListAsync();

        var unreadMessages = messages.Where(m => m.DateRead == null && m.Recipient.UserName == currentUsername).ToList();
        
        if (unreadMessages.Any())
        {
            foreach (var message in unreadMessages)
            {
                message.DateRead = DateTime.UtcNow;
            }
        }
        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public void AddGroup(Group group)
    {
        _context.Groups.Add(group);
    }

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _context.Connections.FindAsync(connectionId);
    }
    public async Task<Group> GetGroup(string groupName)
    {
        return await _context.Groups
            .Include(g => g.Connections)
            .FirstOrDefaultAsync(g => g.Name == groupName);
    }
    public void RemoveConnection(Connection connection)
    {
        _context.Connections.Remove(connection);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _context.Groups
            .Include(g => g.Connections)
            .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
    }
}