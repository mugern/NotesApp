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
                MessageBox.Show($"Не удалось загрузить папки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadNoteData()
        {
            if (_note != null)
            {
                Title = "Редактировать заметку";
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
                        _note.Title = title;
                        _note.Content = content;
                        _note.UpdatedAt = DateTime.UtcNow;
                    }
                    
                    // Ставим папку, если выбрали
                    var selectedFolder = FolderComboBox.SelectedItem as Folder;
                    _note.FolderId = selectedFolder?.Id;
                    
                    context.SaveChanges();
                    
                    MessageBox.Show("Заметка успешно сохранена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить заметку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}