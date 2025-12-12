using System;
using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models
{
    public class Note
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Title { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        public long? FolderId { get; set; }
        public Folder Folder { get; set; }
        
        public long AuthorId { get; set; }
        public User Author { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsShared { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        
        // Функционал корзины, чтобы ничего не потерялось
        public DateTime? DeletedAt { get; set; }
        public long? DeletedBy { get; set; }
        public DateTime? DeletedExpiresAt { get; set; }
    }
}