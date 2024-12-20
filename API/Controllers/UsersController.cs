using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController(IUserRepository userRepository, IMapper mapper) : BaseApiController
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
            if(user == null) return NotFound();
            var memberToReturn = mapper.Map<MemberDto>(user); 
            return memberToReturn;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUserByNameAsync(string username)
        {
            var user = await userRepository.GetMemberByNameAsync(username);  
            if(user == null) return NotFound();
            
            return user;
        }

    }
}