using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models
{
    public class Folder
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public long UserId { get; set; }
        public User User { get; set; }
        
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}