using System;

namespace NotesApp.Models
{
    public class Reminder
    {
        public long Id { get; set; }
        
        public long NoteId { get; set; }
        public Note Note { get; set; }
        
        public DateTime RemindAt { get; set; }
        
        public string Message { get; set; }
        
        public long CreatedById { get; set; }
        public User CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}