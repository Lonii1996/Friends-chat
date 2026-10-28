using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ChatServer.Entities
{
    public class User
    {
        public int Id { get; private set; }
        [Required]
        [MaxLength(20)]
        public string Name { get; set; }
        [Required]
        public string PasswordHash  { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
       
    }
}
