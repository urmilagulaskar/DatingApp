using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper) : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessage)
        {
            var username = User.GetUsername();

            if (username == createMessage.RecipientUsername.ToLower())
                return BadRequest("You can not message yourself");

            var sender = await unitOfWork.UserRepository.GetUserByNameAsync(username);
            var recipient = await unitOfWork.UserRepository.GetUserByNameAsync(createMessage.RecipientUsername);

            if (sender == null || recipient == null || sender.UserName == null || recipient.UserName == null) 
                return BadRequest("We cannot send message at this time");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessage.Content
            };
            unitOfWork.MessageRepository.AddMessage(message);
            if (await unitOfWork.Complete()) return Ok(mapper.Map<MessageDto>(message));

            return BadRequest("Failed to save message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();
            return Ok(await unitOfWork.MessageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await unitOfWork.MessageRepository.GetMessage(id);
            if (message == null) return BadRequest("Can not delete message at this time");

            if (message.SenderUsername != username && message.RecipientUsername != username)
                return Forbid();

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            // before dotnet 8: if(message.SenderDeleted == true || message.RecipientDeleted == true)
            // new way to achieve same in dotnet8 is as below (pattern matching with property pattern)
            if (message is { SenderDeleted: true, RecipientDeleted: true })
            {
                unitOfWork.MessageRepository.DeleteMessage(message);
            }
            if (await unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting the message");
        }

    }


}