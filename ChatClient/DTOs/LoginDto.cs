using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.DTOs
{
    internal class LoginDto
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
