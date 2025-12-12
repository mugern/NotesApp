using System;

namespace NotesApp.Models
{
    public class SharedNote
    {
        public long Id { get; set; }
        
        public long NoteId { get; set; }
        public Note Note { get; set; }
        
        public long UserId { get; set; }
        public User User { get; set; }
        
        public bool CanEdit { get; set; } = true;
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}