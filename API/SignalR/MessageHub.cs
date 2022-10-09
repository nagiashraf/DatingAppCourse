using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub : Hub<IMessageClient>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;
    private readonly UserManager<AppUser> _userManager;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _presenceTracker;

    public MessageHub(IMessageRepository messageRepository, IMapper mapper, UserManager<AppUser> userManager,
        IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker)
    {
        _messageRepository = messageRepository;
        _mapper = mapper;
        _userManager = userManager;
        _presenceHub = presenceHub;
        _presenceTracker = presenceTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var callerUserName = Context.User.FindFirst(ClaimTypes.Name)?.Value;
        var httpContext = Context.GetHttpContext();
        var otherUsername = httpContext.Request.Query["user"].ToString();
        var groupName = GetGroupName(callerUserName, otherUsername);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);
        await Clients.Group(groupName).UpdatedGroup(group);

        var messages = await _messageRepository.GetMessageThreadAsync(callerUserName, otherUsername);

        await Clients.Caller.ReceiveMessgeThread(messages);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var group = await RemoveFromGroup();
        await Clients.Group(group.Name).UpdatedGroup(group);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var senderUsername = Context.User.FindFirst(ClaimTypes.Name)?.Value;

        if (senderUsername == createMessageDto.RecipientUsername.ToLower())
            throw new HubException("You cannot send messages to yourself");

        var sender = await _userManager.FindByNameAsync(senderUsername);
        if (sender is null) throw new HubException("User not found");

        var recipient = await _userManager.FindByNameAsync(createMessageDto.RecipientUsername);
        if (recipient is null) throw new HubException("User not found");

        createMessageDto.Sender = sender;
        createMessageDto.Recipient = recipient;
        
        var message = await _messageRepository.AddMessage(createMessageDto);
        var groupName = GetGroupName(senderUsername, recipient.UserName);
        var group = await _messageRepository.GetGroup(groupName);
        if (group.Connections.Any(c => c.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
            await _messageRepository.SaveAllAsync();
        }
        else
        {
            var recipientConnections = await _presenceTracker.GetUserConnections(recipient.UserName);
            if (recipientConnections != null)
            {
                await _presenceHub.Clients.Clients(recipientConnections)
                    .SendAsync("NewMessageReceived", new { username = sender.UserName, knownAs = sender.KnownAs});
            }
        }

        var messageDto = _mapper.Map<MessageDto>(message);

        await Clients.Group(groupName).NewMessage(_mapper.Map<MessageDto>(message));
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var group = await _messageRepository.GetGroup(groupName);
        var connection = new Connection(Context.ConnectionId, Context.User.FindFirst(ClaimTypes.Name)?.Value);

        if (group == null)
        {
            group = new Group(groupName);
            await _messageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to join the group");
    }

    private async Task<Group> RemoveFromGroup()
    {
        var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
        await _messageRepository.RemoveConnection(connection);

        if (await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to join the group");
    }
}