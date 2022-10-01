using System.Security.Claims;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
    {
        _mapper = mapper;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    [HttpGet("{messageId:int}")]
    public async Task<ActionResult<MessageDto>> GetMessage(int messageId)
    {
        var message = await _messageRepository.GetMessageAsync(messageId);

        if (message is null) return NotFound();

        return _mapper.Map<MessageDto>(message);
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> AddMessage(CreateMessageDto createMessageDto)
    {
        var senderUsername = User.FindFirst(ClaimTypes.Name)?.Value;

        if (senderUsername == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You cannot send messages to yourself");

        var sender = await _userRepository.GetUserByUsernameAsync(senderUsername);
        if (sender is null) return NotFound("User not found");

        var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
        if (recipient is null) return NotFound("User not found");

        createMessageDto.Sender = sender;
        createMessageDto.Recipient = recipient;
        
        var message = await _messageRepository.AddMessage(createMessageDto);

        var messageDto = _mapper.Map<MessageDto>(message);

        return CreatedAtAction(nameof(GetMessage), new { messageId = messageDto.Id }, messageDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        var message = await _messageRepository.GetMessageAsync(id);

        if (message is null) return NotFound("Message not found");

        if (message.SenderUsername != username || message.RecipientUsername != username) return Unauthorized();

        await _messageRepository.DeleteMessage(message, username);

        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        messageParams.Username = username;

        var messages = await _messageRepository.GetMessageForUserAsync(messageParams);

        Response.AddPaginationHeader(messages.PageIndex, messages.PageSize, messages.TotalUsersCount, messages.TotalPagesCount);

        return messages;
    }

    [HttpGet("thread/{recipientUsername}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string recipientUsername)
    {
        var currentUsername = User.FindFirst(ClaimTypes.Name)?.Value;
        var user = await _userRepository.GetUserByUsernameAsync(currentUsername);
        if (user is null) return NotFound("User not found");

        var recipient = await _userRepository.GetUserByUsernameAsync(recipientUsername);
        if (recipient is null) return NotFound("User not found");

        var messages = await _messageRepository.GetMessageThreadAsync(currentUsername, recipientUsername);

        return Ok(messages);
    }
}