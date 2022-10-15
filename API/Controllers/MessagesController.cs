using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MessagesController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("{messageId:int}")]
    public async Task<ActionResult<MessageDto>> GetMessage(int messageId)
    {
        var message = await _unitOfWork.MessageRepository.GetMessageAsync(messageId);

        if (message is null) return NotFound();

        return _mapper.Map<MessageDto>(message);
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> AddMessage(CreateMessageDto createMessageDto)
    {
        var senderUsername = User.FindFirst(ClaimTypes.Name)?.Value;

        if (senderUsername == createMessageDto.RecipientUsername.ToLower())
            return BadRequest("You cannot send messages to yourself");

        var sender = await _userManager.FindByNameAsync(senderUsername);
        if (sender is null) return NotFound("User not found");

        var recipient = await _userManager.FindByNameAsync(createMessageDto.RecipientUsername);
        if (recipient is null) return NotFound("User not found");

        createMessageDto.Sender = sender;
        createMessageDto.Recipient = recipient;
        
        var message = _unitOfWork.MessageRepository.AddMessage(createMessageDto);
        await _unitOfWork.Complete();

        var messageDto = _mapper.Map<MessageDto>(message);

        return CreatedAtAction(nameof(GetMessage), new { messageId = messageDto.Id }, messageDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        var message = await _unitOfWork.MessageRepository.GetMessageAsync(id);

        if (message is null) return NotFound("Message not found");

        if (message.SenderUsername != username || message.RecipientUsername != username) return Unauthorized();

        _unitOfWork.MessageRepository.DeleteMessage(message, username);
        await _unitOfWork.Complete();

        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        messageParams.Username = username;

        var messages = await _unitOfWork.MessageRepository.GetMessageForUserAsync(messageParams);

        Response.AddPaginationHeader(messages.PageIndex, messages.PageSize, messages.TotalUsersCount, messages.TotalPagesCount);

        return messages;
    }
}