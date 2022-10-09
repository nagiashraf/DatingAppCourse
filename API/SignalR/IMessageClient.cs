using API.DTOs;
using API.Entities;

namespace API.SignalR;

public interface IMessageClient
{
    Task ReceiveMessgeThread(IEnumerable<MessageDto> messages);
    Task NewMessage(MessageDto messageDto);
    Task UpdatedGroup(Group group);
}