using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NotesApp.Data;
using NotesApp.Models;
using MaterialDesignThemes.Wpf;

namespace NotesApp.Views
{
    public partial class MainWindow : Window
    {
        private User _currentUser;
        private Folder _selectedFolder;
        private List<Note> _notes;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            this.Title = $"Приложение для заметок - Добро пожаловать {_currentUser.Username}";
            
            // Показываем админское меню, если юзер админ
            if (_currentUser.IsAdmin)
            {
                AdminMenu.Visibility = Visibility.Visible;
            }
            
            LoadFolders();
            LoadNotes();
        }

        private void LoadFolders()
        {
            try
            {
                using (var context = new NotesAppContext())
                {
                    var folders = context.Folders
                        .Where(f => f.UserId == _currentUser.Id && f.Notes.All(n => n.DeletedAt == null))
                        .ToList();
                    
                    FoldersList.ItemsSource = folders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить папки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadNotes()
        {
            try
            {
                using (var context = new NotesAppContext())
                {
                    IQueryable<Note> query;
                    
                    if (_selectedFolder != null)
                    {
                        query = context.Notes
                            .Where(n => n.FolderId == _selectedFolder.Id && n.DeletedAt == null);
                    }
                    else
                    {
                        query = context.Notes
                            .Where(n => n.AuthorId == _currentUser.Id && n.DeletedAt == null);
                    }
                    
                    _notes = query.ToList();
                    NotesDataGrid.ItemsSource = _notes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FolderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedFolder = FoldersList.SelectedItem as Folder;
            LoadNotes();
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            // Диалог создания папки, как в старом добром DOS
            string folderName = Microsoft.VisualBasic.Interaction.InputBox("Введите название папки:", "Создать папку", "");
            
            if (!string.IsNullOrEmpty(folderName))
            {
                try
                {
                    using (var context = new NotesAppContext())
                    {
                        var folder = new Folder
                        {
                            Name = folderName,
                            UserId = _currentUser.Id
                        };
                        
                        context.Folders.Add(folder);
                        context.SaveChanges();
                        
                        LoadFolders();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось создать папку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CreateNote_Click(object sender, RoutedEventArgs e)
        {
            var noteEditor = new NoteEditorWindow(_currentUser, null);
            noteEditor.ShowDialog();
            LoadNotes();
        }

        private void EditNote_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (NotesDataGrid.SelectedItem is Note selectedNote)
            {
                var noteEditor = new NoteEditorWindow(_currentUser, selectedNote);
                noteEditor.ShowDialog();
                LoadNotes();
            }
        }

        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            if (NotesDataGrid.SelectedItem is Note selectedNote)
            {
                var noteEditor = new NoteEditorWindow(_currentUser, selectedNote);
                noteEditor.ShowDialog();
                LoadNotes();
            }
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (NotesDataGrid.SelectedItem is Note selectedNote)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить '{selectedNote.Title}'?", "Подтвердить удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new NotesAppContext())
                        {
                            var note = context.Notes.FirstOrDefault(n => n.Id == selectedNote.Id);
                            if (note != null)
                            {
                                note.DeletedAt = DateTime.UtcNow;
                                note.DeletedBy = _currentUser.Id;
                                context.SaveChanges();
                                
                                LoadNotes();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось удалить заметку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ArchiveNote_Click(object sender, RoutedEventArgs e)
        {
            if (NotesDataGrid.SelectedItem is Note selectedNote)
            {
                try
                {
                    using (var context = new NotesAppContext())
                    {
                        var note = context.Notes.FirstOrDefault(n => n.Id == selectedNote.Id);
                        if (note != null)
                        {
                            note.IsArchived = !note.IsArchived;
                            context.SaveChanges();
                            
                            LoadNotes();
                            
                            string action = note.IsArchived ? "archived" : "unarchived";
                            string actionRu = note.IsArchived ? "архивирована" : "разархивирована";
                            MessageBox.Show($"Заметка была {actionRu}.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось архивировать заметку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewTrash_Click(object sender, RoutedEventArgs e)
        {
            var trashWindow = new TrashWindow(_currentUser);
            trashWindow.ShowDialog();
            LoadNotes();
        }

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.IsAdmin)
            {
                var userManagementWindow = new UserManagementWindow();
                userManagementWindow.ShowDialog();
            }
        }

        private void SendNotification_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser.IsAdmin)
            {
                var notificationWindow = new NotificationWindow();
                notificationWindow.ShowDialog();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NotesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = NotesDataGrid.SelectedItem != null;
            EditNoteButton.IsEnabled = hasSelection;
            DeleteNoteButton.IsEnabled = hasSelection;
            ArchiveNoteButton.IsEnabled = hasSelection;
        }

        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme("Light");
        }

        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme("Dark");
        }

        private void ApplyTheme(string theme)
        {
            var paletteHelper = new PaletteHelper();
            var existingTheme = paletteHelper.GetTheme();

            if (theme == "Dark")
            {
                existingTheme.SetBaseTheme(Theme.Dark);
            }
            else
            {
                existingTheme.SetBaseTheme(Theme.Light);
            }

            paletteHelper.SetTheme(existingTheme);
        }
    }
}