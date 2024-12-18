using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class UserDto
    {
        public required string UserName { get; set; }
        public required string Token { get; set; }
    }
}