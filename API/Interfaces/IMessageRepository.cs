using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{
    Task<Message> AddMessage(CreateMessageDto createMessageDto);
    Task DeleteMessage(Message message, string username);
    Task<Message> GetMessageAsync(int id);
    Task<PagedList<MessageDto>> GetMessageForUserAsync(MessageParams messageParams);
    Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUsername, string recipientUsername);
}