using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUserRepository userRepository, IMapper mapper,
        IPhotoService photoService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsersAsync()
        {
            var users = await userRepository.GetMembersAsync();
            return Ok(users);
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<MemberDto>> GetUserByIdAsync(int id)
        {
            var user = await userRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            var memberToReturn = mapper.Map<MemberDto>(user);
            return memberToReturn;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUserByNameAsync(string username)
        {
            var user = await userRepository.GetMemberByNameAsync(username);
            if (user == null) return NotFound();

            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null) return BadRequest("No Username found in token");

            var user = await userRepository.GetUserByNameAsync(username);
            if (user == null) return BadRequest("Could not find user");
            mapper.Map(memberUpdateDto, user);
            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user profile");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await userRepository.GetUserByNameAsync(User.GetUsername());
            if (user == null) return BadRequest("Cannot update user");
            var result = await photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            user.Photos.Add(photo);
            if (await userRepository.SaveAllAsync()) return mapper.Map<PhotoDto>(photo);
            /*             if (await userRepository.SaveAllAsync())
                            return CreatedAtAction(nameof(this.GetUserByNameAsync),
                               new { username = user.UserName }, mapper.Map<PhotoDto>(photo)); */
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByNameAsync(User.GetUsername());
            if (user == null) return BadRequest("Could not find user");
            var photo = user.Photos.FirstOrDefault(x=> x.Id == photoId);
            if(photo == null || photo.IsMain) return BadRequest("Cannot use this as Main photo");

            var currentMain = user.Photos.FirstOrDefault(x=> x.IsMain);
            if(currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;
            if(await userRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUserByNameAsync(User.GetUsername());
            if(user==null) return BadRequest("User not found");
            var photo = user.Photos.FirstOrDefault(x=> x.Id == photoId);
            if(photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message); 
            }

            user.Photos.Remove(photo);
            if(await userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting photo");
        }

    }
}