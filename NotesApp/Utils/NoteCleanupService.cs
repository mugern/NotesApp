using System;
using System.Linq;
using NotesApp.Data;
using NotesApp.Models;

namespace NotesApp.Utils
{
    public class NoteCleanupService
    {      
        /// Удаление истекших заметок
        /// <returns>Кол-во удаленных заметок</returns>
        public static int DeleteExpiredNotes()
        {
            try
            {
                using (var context = new NotesAppContext())
                {
                    var expiredNotes = context.Notes
                        .Where(n => n.DeletedAt != null && n.DeletedExpiresAt < DateTime.UtcNow)
                        .ToList();
                    
                    int count = expiredNotes.Count;
                    
                    foreach (var note in expiredNotes)
                    {
                        context.Notes.Remove(note);
                    }
                    
                    context.SaveChanges();
                    return count;
                }
            }
            catch (Exception)
            {
                // Ошибка - возвращаем 0
                return 0;
            }
        }
        public static int CleanupNotes()
        {
            return DeleteExpiredNotes();
        }
    }
}