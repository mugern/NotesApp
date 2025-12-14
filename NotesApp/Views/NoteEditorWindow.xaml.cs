using System;
using System.Linq;
using System.Windows;
using NotesApp.Data;
using NotesApp.Models;

namespace NotesApp.Views
{
    public partial class NoteEditorWindow : Window
    {
        private User _currentUser;
        private Note _note;
        
        private void LogError(Exception ex, string methodName)
        {
            // В реальном приложении здесь должно быть полноценное логирование
            System.Diagnostics.Debug.WriteLine($"Ошибка в методе {methodName}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
        }

        public NoteEditorWindow(User user, Note note)
        {
            InitializeComponent();
            _currentUser = user;
            _note = note;
            
            LoadFolders();
            LoadNoteData();
        }

        private void LoadFolders()
        {
            try
            {
                using (var context = new NotesAppContext())
                {
                    var folders = context.Folders
                        .Where(f => f.UserId == _currentUser.Id)
                        .ToList();
                    
                    FolderComboBox.ItemsSource = folders;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "LoadFolders");
                MessageBox.Show($"Не удалось загрузить папки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Продолжаем работу приложения, не прерывая его
                FolderComboBox.ItemsSource = null;
            }
        }

        private void LoadNoteData()
        {
            if (_note != null)
            {
                Title = "Редактировать заметку";
                WindowTitleTextBlock.Text = "Редактировать заметку";
                TitleTextBox.Text = _note.Title;
                ContentTextBox.Text = _note.Content;
                
                // Ставим папку в выпадайку
                if (_note.FolderId.HasValue)
                {
                    using (var context = new NotesAppContext())
                    {
                        var folder = context.Folders.FirstOrDefault(f => f.Id == _note.FolderId.Value);
                        if (folder != null)
                        {
                            FolderComboBox.SelectedItem = folder;
                        }
                    }
                }
            }
            else
            {
                Title = "Создать заметку";
                WindowTitleTextBlock.Text = "Создать заметку";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            string content = ContentTextBox.Text;
            
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Пожалуйста, введите заголовок для заметки.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                if (FolderComboBox.SelectedItem != null)
                {
                    using (var context = new NotesAppContext())
                    {
                        if (_note == null)
                        {
                            // Делаем новую заметку
                            _note = new Note
                            {
                                Title = title,
                                Content = content,
                                AuthorId = _currentUser.Id,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };

                            context.Notes.Add(_note);
                        }
                        else
                        {
                            // Правим старую заметку
                            // Загружаем заметку заново из базы данных
                            var noteToUpdate = context.Notes.Find(_note.Id);
                            if (noteToUpdate != null)
                            {
                                noteToUpdate.Title = title;
                                noteToUpdate.Content = content;
                                noteToUpdate.UpdatedAt = DateTime.UtcNow;
                                // Обновляем папку для существующей заметки
                                var selectedFolder = FolderComboBox.SelectedItem as Folder;
                                noteToUpdate.FolderId = selectedFolder?.Id;
                            }
                        }

                        // Устанавливаем папку для новой заметки
                        if (_note != null && _note.Id == 0) // Новая заметка
                        {
                            var selectedFolder = FolderComboBox.SelectedItem as Folder;
                            _note.FolderId = selectedFolder?.Id;
                        }

                        context.SaveChanges();

                        MessageBox.Show("Заметка успешно сохранена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите папку, сударь", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "SaveButton_Click");
                MessageBox.Show($"Не удалось сохранить заметку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Не закрываем окно, чтобы пользователь мог попробовать снова
                // Продолжаем работу приложения, не прерывая его
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}