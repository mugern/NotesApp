using System;
using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models
{
    public class User
    {
        public long Id { get; set; }
        
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsAdmin { get; set; } = false;
    }
}