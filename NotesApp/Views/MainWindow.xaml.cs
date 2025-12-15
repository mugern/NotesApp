using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;

namespace NotesApp.Views
{
    public partial class MainWindow : Window
    {
        private User _currentUser;
        private List<Note> _notes;
        private Folder _selectedFolder;
        private Note _selectedNote;
        private Border _selectedNoteBorder;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            // Отображение имени пользователя
            if (UsernameTextBlock != null)
            {
                UsernameTextBlock.Text = _currentUser.Username;
            }
            LoadFolders();
            LoadNotes();
        }

        private void LogError(Exception ex, string methodName)
        {
            // Логирование ошибок
            System.Diagnostics.Debug.WriteLine($"Ошибка в методе {methodName}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
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
                    
                    FoldersList.ItemsSource = folders;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "LoadFolders");
                MessageBox.Show($"Не удалось загрузить папки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Сброс выбора папки
                FoldersList.ItemsSource = null;
                _selectedFolder = null;
                // Продолжение работы
            }
        }

        private void LoadNotes(bool resetSelection = true)
        {
            try
            {
                using (var context = new NotesAppContext())
                {
                    IQueryable<Note> query = context.Notes
                        .Include(n => n.Folder) // Данные папки
                        .Where(n => n.AuthorId == _currentUser.Id && n.DeletedAt == null && n.IsArchived == false);
                    
                    // Фильтр по папке
                    if (_selectedFolder != null)
                    {
                        query = query.Where(n => n.FolderId == _selectedFolder.Id);
                    }
                    
                    _notes = query.ToList();
                    // Задержка для загрузки
                    System.Threading.Thread.Sleep(10);
                    DisplayNotes(resetSelection);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "LoadNotes");
                MessageBox.Show($"Не удалось загрузить заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Пустой список заметок
                _notes = new List<Note>();
                DisplayNotes(resetSelection);
                // Продолжение работы
            }
        }

        private void DisplayNotes(bool resetSelection = true)
        {
            // Сброс выбора заметки
            if (resetSelection)
            {
                ResetNoteSelection();
            }
            
            // Обновление заметок
            for (int i = 0; i < _notes.Count; i++)
            {
                var note = _notes[i];
                Border noteBorder;
                StackPanel notePanel;
                TextBlock folderTextBlock, titleTextBlock, contentTextBlock, dateTextBlock;
                
                // Проверка существующего элемента
                if (i < BackgroundNotes.Children.Count && BackgroundNotes.Children[i] is Border existingBorder)
                {
                    // Используем существующий
                    noteBorder = existingBorder;
                    notePanel = noteBorder.Child as StackPanel;
                    
                    // Обновляем содержимое
                    if (notePanel != null && notePanel.Children.Count >= 4)
                    {
                        folderTextBlock = notePanel.Children[0] as TextBlock;
                        titleTextBlock = notePanel.Children[1] as TextBlock;
                        contentTextBlock = notePanel.Children[2] as TextBlock;
                        dateTextBlock = notePanel.Children[3] as TextBlock;
                        
                        if (folderTextBlock != null) folderTextBlock.Text = note.Folder?.Name ?? "Без папки";
                        if (titleTextBlock != null) titleTextBlock.Text = note.Title;
                        if (contentTextBlock != null) contentTextBlock.Text = note.Content;
                        if (dateTextBlock != null) dateTextBlock.Text = $"Создано: {note.CreatedAt:dd.MM.yyyy}";
                    }
                    else
                    {
                        // Создаем заново
                        notePanel = CreateNotePanel(note, out folderTextBlock, out titleTextBlock, out contentTextBlock, out dateTextBlock);
                        noteBorder.Child = notePanel;
                    }
                    
                    // Обновляем обработчик
                    noteBorder.MouseLeftButtonUp -= (sender, e) => SelectNote(note, noteBorder);
                    noteBorder.MouseLeftButtonUp += (sender, e) => SelectNote(note, noteBorder);
                }
                else
                {
                    // Создаем новый элемент
                    noteBorder = new Border
                    {
                        MinWidth = 180,
                        Height = Double.NaN, // Автоматическая высота
                        Margin = new Thickness(5),
                        Background = new SolidColorBrush(Colors.LightBlue),
                        BorderBrush = new SolidColorBrush(Colors.DarkBlue),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(3),
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    
                    notePanel = CreateNotePanel(note, out folderTextBlock, out titleTextBlock, out contentTextBlock, out dateTextBlock);
                    noteBorder.Child = notePanel;
                    
                    // Добавляем обработчик
                    noteBorder.MouseLeftButtonUp += (sender, e) => SelectNote(note, noteBorder);
                    
                    // Добавляем в панель
                    BackgroundNotes.Children.Add(noteBorder);
                }
                
                // Сохраняем ссылку на заметку
                if (noteBorder != null)
                {
                    noteBorder.Tag = note.Id;
                }
            }
            
            // Удаление лишних элементов
            while (BackgroundNotes.Children.Count > _notes.Count)
            {
                BackgroundNotes.Children.RemoveAt(BackgroundNotes.Children.Count - 1);
            }
            
            // Восстановление выбора
            if (!resetSelection && _selectedNote != null)
            {
                RestoreNoteSelection();
            }
        }
        
        private void ResetNoteSelection()
        {
            // Сброс выбора
            if (_selectedNoteBorder != null)
            {
                _selectedNoteBorder.Background = new SolidColorBrush(Colors.LightBlue);
                _selectedNoteBorder.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
                _selectedNoteBorder.BorderThickness = new Thickness(1);
            }
            _selectedNote = null;
            _selectedNoteBorder = null;
            EditNoteButton.IsEnabled = false;
            DeleteNoteButton.IsEnabled = false;
        }
        
        private void RestoreNoteSelection()
        {
            if (_selectedNote != null && BackgroundNotes.Children.Count > 0)
            {
                // Поиск заметки по ID
                foreach (Border noteBorder in BackgroundNotes.Children)
                {
                    if (noteBorder.Tag is long noteId && noteId == _selectedNote.Id)
                    {
                        // Найдена заметка
                        var note = _notes.FirstOrDefault(n => n.Id == noteId);
                        if (note != null)
                        {
                            SelectNote(note, noteBorder);
                            break;
                        }
                    }
                }
            }
        }
        
        private void UpdateNoteDisplay(Note updatedNote)
        {
            // Поиск элемента по ID
            foreach (Border noteBorder in BackgroundNotes.Children)
            {
                if (noteBorder.Tag is long noteId && noteId == updatedNote.Id)
                {
                    // Найден элемент
                    var notePanel = noteBorder.Child as StackPanel;
                    
                    // Обновление содержимого
                    if (notePanel != null && notePanel.Children.Count >= 4)
                    {
                        var folderTextBlock = notePanel.Children[0] as TextBlock;
                        var titleTextBlock = notePanel.Children[1] as TextBlock;
                        var contentTextBlock = notePanel.Children[2] as TextBlock;
                        var dateTextBlock = notePanel.Children[3] as TextBlock;
                        
                        if (folderTextBlock != null) folderTextBlock.Text = updatedNote.Folder?.Name ?? "Без папки";
                        if (titleTextBlock != null) titleTextBlock.Text = updatedNote.Title;
                        if (contentTextBlock != null) contentTextBlock.Text = updatedNote.Content;
                        if (dateTextBlock != null) dateTextBlock.Text = $"Создано: {updatedNote.CreatedAt:dd.MM.yyyy}";
                    }
                    break;
                }
            }
        }
        
        private StackPanel CreateNotePanel(Note note, out TextBlock folderTextBlock, out TextBlock titleTextBlock, out TextBlock contentTextBlock, out TextBlock dateTextBlock)
        {
            // Создание панели заметки
            var notePanel = new StackPanel
            {
                Margin = new Thickness(5)
            };
            
            // Текстовое поле папки
            folderTextBlock = new TextBlock
            {
                Text = note.Folder?.Name ?? "Без папки",
                FontStyle = FontStyles.Italic,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.DarkBlue),
                Margin = new Thickness(0, 0, 0, 3),
                TextWrapping = TextWrapping.Wrap
            };
            
            // Текстовое поле заголовка
            titleTextBlock = new TextBlock
            {
                Text = note.Title,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 3),
                TextWrapping = TextWrapping.Wrap
            };
            
            // Текстовое поле содержимого
            contentTextBlock = new TextBlock
            {
                Text = note.Content,
                FontSize = 10,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 3)
            };
            
            // Текстовое поле даты
            dateTextBlock = new TextBlock
            {
                Text = $"Создано: {note.CreatedAt:dd.MM.yyyy}",
                FontSize = 9,
                Foreground = new SolidColorBrush(Colors.Gray)
            };
            
            // Добавление элементов
            notePanel.Children.Add(folderTextBlock);
            notePanel.Children.Add(titleTextBlock);
            notePanel.Children.Add(contentTextBlock);
            notePanel.Children.Add(dateTextBlock);
            
            return notePanel;
        }
        
        private void SelectNote(Note note, Border noteBorder)
        {
            // Сброс выделения
            if (_selectedNoteBorder != null)
            {
                _selectedNoteBorder.Background = new SolidColorBrush(Colors.LightBlue);
                _selectedNoteBorder.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
                _selectedNoteBorder.BorderThickness = new Thickness(1);
            }
            
            // Установка выбранной заметки
            _selectedNote = note;
            _selectedNoteBorder = noteBorder;
            
            // Выделение заметки
            _selectedNoteBorder.Background = new SolidColorBrush(Colors.LightGreen);
            _selectedNoteBorder.BorderBrush = new SolidColorBrush(Colors.DarkGreen);
            _selectedNoteBorder.BorderThickness = new Thickness(2);
            
            // Активация кнопок
            EditNoteButton.IsEnabled = true;
            DeleteNoteButton.IsEnabled = true;
        }
        
        private void FolderSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedFolder = FoldersList.SelectedItem as Folder;
            LoadNotes();
        }

        private void CreateNote_Click(object sender, RoutedEventArgs e)
        {
            var noteEditor = new NoteEditorWindow(_currentUser, null);
            if (noteEditor.ShowDialog() == true)
            {
                LoadNotes();
                LoadFolders(); // Обновление папок
            }
        }

        private void EditNote(Note note)
        {
            var noteEditor = new NoteEditorWindow(_currentUser, note);
            if (noteEditor.ShowDialog() == true)
            {
                // Перезагрузка заметок
                LoadNotes(false); // Без сброса выбора
                
                LoadFolders(); // Обновление папок
            }
        }

        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNote != null)
            {
                var noteEditor = new NoteEditorWindow(_currentUser, _selectedNote);
                if (noteEditor.ShowDialog() == true)
                {
                    // Перезагрузка заметок
                    LoadNotes(false); // Без сброса выбора
                    
                    LoadFolders(); // Обновление папок
                }
            }
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNote != null)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить '{_selectedNote.Title}'? Заметка будет перемещена в корзину и окончательно удалена через 30 дней.", "Подтвердить удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new NotesAppContext())
                        {
                            var note = context.Notes.FirstOrDefault(n => n.Id == _selectedNote.Id);
                            if (note != null)
                            {
                                // Мягкое удаление
                                note.DeletedAt = DateTime.UtcNow;
                                note.DeletedBy = _currentUser.Id;
                                note.DeletedExpiresAt = DateTime.UtcNow.AddDays(30);
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
            MessageBox.Show("Функция архивирования заметки будет реализована позже.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            var createFolderWindow = new CreateFolderWindow(_currentUser);
            if (createFolderWindow.ShowDialog() == true)
            {
                LoadFolders();
                LoadNotes();
            }
        }
        
        private void ShowAllFolders_Click(object sender, RoutedEventArgs e)
        {
            FoldersList.SelectedItem = null;
            _selectedFolder = null;
            LoadNotes();
        }
        
        private void ViewTrash_Click(object sender, RoutedEventArgs e)
        {
            var trashWindow = new TrashWindow(_currentUser);
            trashWindow.ShowDialog();
        }

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            var userManagementWindow = new UserManagementWindow();
            userManagementWindow.ShowDialog();
        }

        private void SendNotification_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция отправки уведомлений будет реализована позже.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            // Реализация темной темы
            BackgroundNotes.Background = new SolidColorBrush(Colors.DarkSlateBlue);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void RenameFolder_Click(object sender, RoutedEventArgs e)
        {
            // Получение папки из MenuItem
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var stackPanel = contextMenu?.PlacementTarget as StackPanel;
            var folder = stackPanel?.DataContext as Folder;
            
            if (folder != null)
            {
                string newName = Microsoft.VisualBasic.Interaction.InputBox("Введите новое название папки:", "Переименовать папку", folder.Name);
                if (!string.IsNullOrWhiteSpace(newName) && newName != folder.Name)
                {
                    try
                    {
                        using (var context = new NotesAppContext())
                        {
                            // Проверка существующей папки
                            var existingFolder = context.Folders.FirstOrDefault(f => f.Name == newName && f.UserId == _currentUser.Id);
                            if (existingFolder != null)
                            {
                                MessageBox.Show("Папка с таким названием уже существует. Пожалуйста, выберите другое название.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            
                            var folderToUpdate = context.Folders.FirstOrDefault(f => f.Id == folder.Id);
                            if (folderToUpdate != null)
                            {
                                folderToUpdate.Name = newName;
                                context.SaveChanges();
                                LoadFolders();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось переименовать папку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        
        private void DeleteFolder_Click(object sender, RoutedEventArgs e)
        {
            // Получение папки из MenuItem
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var stackPanel = contextMenu?.PlacementTarget as StackPanel;
            var folder = stackPanel?.DataContext as Folder;
            
            if (folder != null)
            {
                // Проверка наличия заметок
                using (var dbContext = new NotesAppContext())
                {
                    var notesCount = dbContext.Notes.Count(n => n.FolderId == folder.Id && n.DeletedAt == null);
                    if (notesCount > 0)
                    {
                        var messageResult = MessageBox.Show($"В папке '{folder.Name}' есть {notesCount} заметок. Хотите удалить папку вместе со всеми заметками? Все заметки будут удалены безвозвратно.", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        
                        if (messageResult == MessageBoxResult.Yes)
                        {
                            try
                            {
                                using (var deleteContext = new NotesAppContext())
                                {
                                    using var transaction = deleteContext.Database.BeginTransaction();
                                    
                                    // Удаление заметок из папки
                                    var notes = deleteContext.Notes.Where(n => n.FolderId == folder.Id).ToList();
                                    foreach (var note in notes)
                                    {
                                        deleteContext.Notes.Remove(note);
                                    }
                                    
                                    // Сохранение изменений
                                    deleteContext.SaveChanges();
                                    
                                    // Удаление папки
                                    var folderToDelete = deleteContext.Folders.FirstOrDefault(f => f.Id == folder.Id);
                                    if (folderToDelete != null)
                                    {
                                        deleteContext.Folders.Remove(folderToDelete);
                                    }
                                    
                                    deleteContext.SaveChanges();
                                    transaction.Commit();
                                    
                                    LoadFolders();
                                    LoadNotes();
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Не удалось удалить папку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        // Удаление пустой папки
                        var result = MessageBox.Show($"Вы уверены, что хотите удалить папку '{folder.Name}'?", "Подтвердить удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                using (var context = new NotesAppContext())
                                {
                                    var folderToDelete = context.Folders.FirstOrDefault(f => f.Id == folder.Id);
                                    if (folderToDelete != null)
                                    {
                                        context.Folders.Remove(folderToDelete);
                                        context.SaveChanges();
                                        LoadFolders();
                                        LoadNotes();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Не удалось удалить папку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
        }
    }
}