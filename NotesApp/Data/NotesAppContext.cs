using System;
using Microsoft.EntityFrameworkCore;
using NotesApp.Models;

namespace NotesApp.Data
{
    public class NotesAppContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<SharedNote> SharedNotes { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        public static void LogError(Exception ex, string methodName)
        {
            // В реальном приложении здесь должно быть полноценное логирование
            System.Diagnostics.Debug.WriteLine($"Ошибка в методе {methodName}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            try
            {
                options.UseNpgsql("Host=node-6b2e4.smrgames.ru;Port=58002;Database=app;Username=postgres;Password=Pl3453Ch4n63M3!");
            }
            catch (Exception ex)
            {
                LogError(ex, "OnConfiguring");
                throw; // Пробрасываем исключение дальше, чтобы приложение могло обработать его правильно
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настраиваем сущность пользователя,
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            });           
            // Настраиваем сущность папки, чтобы не было путаницы
            modelBuilder.Entity<Folder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                
                // Настраиваем связь с пользователем, чтобы не путалось ничего
                entity.HasOne(f => f.User)
                    .WithMany()
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настраиваем сущность заметки
            modelBuilder.Entity<Note>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
                entity.Property(e => e.IsShared).HasDefaultValue(false);
                entity.Property(e => e.IsArchived).HasDefaultValue(false);

                // Настраиваем связи, чтобы все было по красоте
                entity.HasOne(n => n.Folder)
                    .WithMany(f => f.Notes)
                    .HasForeignKey(n => n.FolderId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(n => n.Author)
                    .WithMany()
                    .HasForeignKey(n => n.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настраиваем сущность общих заметок
            modelBuilder.Entity<SharedNote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CanEdit).HasDefaultValue(true);
                entity.Property(e => e.AddedAt).HasDefaultValueSql("now()");

                // Настраиваем связи, чтобы не было путаницы
                entity.HasOne(sn => sn.Note)
                    .WithMany()
                    .HasForeignKey(sn => sn.NoteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sn => sn.User)
                    .WithMany()
                    .HasForeignKey(sn => sn.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Убеждаемся, что комбинация NoteId и UserId уникальна, чтобы не было дублирующихся записей в базе данных
                entity.HasIndex(sn => new { sn.NoteId, sn.UserId }).IsUnique();
            });

            // Настраиваем сущность напоминаний, чтобы никто ничего не забыл
            modelBuilder.Entity<Reminder>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Настраиваем связи, чтобы напоминания приходили правильным людям
                entity.HasOne(r => r.Note)
                    .WithMany()
                    .HasForeignKey(r => r.NoteId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.CreatedBy)
                    .WithMany()
                    .HasForeignKey(r => r.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Создаем индексы для ускорения работы
            modelBuilder.Entity<Folder>()
                .HasIndex(f => f.UserId)
                .HasDatabaseName("idx_folders_user");

            modelBuilder.Entity<Note>()
                .HasIndex(n => n.FolderId)
                .HasDatabaseName("idx_notes_folder");

            modelBuilder.Entity<Note>()
                .HasIndex(n => n.AuthorId)
                .HasDatabaseName("idx_notes_author");

            modelBuilder.Entity<Note>()
                .HasIndex(n => n.DeletedAt)
                .HasDatabaseName("idx_notes_deleted")
                .HasFilter("\"DeletedAt\" IS NOT NULL");

            modelBuilder.Entity<Note>()
                .HasIndex(n => n.DeletedExpiresAt)
                .HasDatabaseName("idx_notes_purge")
                .HasFilter("\"DeletedAt\" IS NOT NULL");

            modelBuilder.Entity<Note>()
                .HasIndex(n => new { n.FolderId, n.Title })
                .HasDatabaseName("uq_notes_folder_title_active")
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");

            modelBuilder.Entity<SharedNote>()
                .HasIndex(sn => sn.UserId)
                .HasDatabaseName("idx_shared_by_user");

            modelBuilder.Entity<Reminder>()
                .HasIndex(r => r.RemindAt)
                .HasDatabaseName("idx_reminders_when");

            modelBuilder.Entity<Reminder>()
                .HasIndex(r => r.NoteId)
                .HasDatabaseName("idx_reminders_note");
        }
    }
}