using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;

namespace NotesApp.Views
{
    public partial class TrashWindow : Window
    {
        private User _currentUser;
        private List<Note> _trashNotes;
        private List<Folder> _folders;
        private Note _selectedNote;

        public TrashWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadFolders();
            LoadTrashNotes();
        }

        private void LoadFolders()
        {
            try
            {
                using (var context = new NotesAppContext())
                {
                    _folders = context.Folders
                        .Where(f => f.UserId == _currentUser.Id)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить папки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTrashNotes()
        {
            try
            {
                using (var context = new NotesAppContext())
                {
                    _trashNotes = context.Notes
                        .Include(n => n.Folder) // Данные папки
                        .Where(n => n.AuthorId == _currentUser.Id && n.DeletedAt != null)
                        .OrderByDescending(n => n.DeletedAt) // Сортировка по дате
                        .ToList();
                    
                    DisplayTrashNotes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить корзину: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayTrashNotes()
        {
            // Очистка панели
            TrashNotesPanel.Children.Clear();
            
            // Создание элементов удаленных заметок
            foreach (var note in _trashNotes)
            {
                // Контейнер заметки
                Border noteBorder = new Border
                {
                    Width = 200,
                    Height = 160,
                    Margin = new Thickness(10),
                    Background = new SolidColorBrush(Colors.LightCoral),
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(10)
                };
                
                // Панель содержимого
                StackPanel notePanel = new StackPanel
                {
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                // Текстовое поле заголовка
                TextBlock titleTextBlock = new TextBlock
                {
                    Text = note.Title,
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 5),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.Black)
                };
                
                // Текстовое поле папки
                TextBlock folderTextBlock = new TextBlock
                {
                    Text = note.Folder?.Name ?? "Без папки",
                    FontStyle = FontStyles.Italic,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Colors.Black),
                    Margin = new Thickness(0, 0, 0, 5),
                    TextWrapping = TextWrapping.Wrap
                };
                
                // Текстовое поле даты удаления
                TextBlock deletedTextBlock = new TextBlock
                {
                    Text = note.DeletedAt.HasValue ?
                        $"Удалено: {note.DeletedAt.Value:dd.MM.yyyy HH:mm}" :
                        "Удалено: Не указано",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Black),
                    Margin = new Thickness(0, 0, 0, 3)
                };
                
                // Текстовое поле даты истечения
                TextBlock expiresTextBlock = new TextBlock
                {
                    Text = note.DeletedExpiresAt.HasValue ?
                        $"Истекает через: {Math.Max(0, (note.DeletedExpiresAt.Value - DateTime.UtcNow).Days)} дней" :
                        "Истекает: Не указано",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Black)
                };
                
                // Добавление элементов
                notePanel.Children.Add(titleTextBlock);
                notePanel.Children.Add(folderTextBlock);
                notePanel.Children.Add(deletedTextBlock);
                notePanel.Children.Add(expiresTextBlock);
                
                // Добавление панели в контейнер
                noteBorder.Child = notePanel;
                
                // Обработчик клика
                noteBorder.MouseLeftButtonUp += (sender, e) => SelectNote(note, noteBorder);
                
                // Добавление в панель
                TrashNotesPanel.Children.Add(noteBorder);
            }
            
            // Сброс выбора
            _selectedNote = null;
            RestoreButton.IsEnabled = false;
            RestoreToFolderButton.IsEnabled = false;
            DeletePermanentlyButton.IsEnabled = false;
        }

        private void SelectNote(Note note, Border border)
        {
            // Сброс выделения
            foreach (UIElement element in TrashNotesPanel.Children)
            {
                if (element is Border b)
                {
                    b.BorderBrush = new SolidColorBrush(Colors.DarkRed);
                    b.BorderThickness = new Thickness(2);
                }
            }
            
            // Выделение заметки
            border.BorderBrush = new SolidColorBrush(Colors.Green);
            border.BorderThickness = new Thickness(3);
            
            // Сохранение выбранной заметки
            _selectedNote = note;
            RestoreButton.IsEnabled = true;
            RestoreToFolderButton.IsEnabled = true;
            DeletePermanentlyButton.IsEnabled = true;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNote != null)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите восстановить '{_selectedNote.Title}'?", "Подтвердить восстановление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new NotesAppContext())
                        {
                            var note = context.Notes.FirstOrDefault(n => n.Id == _selectedNote.Id);
                            if (note != null)
                            {
                                note.DeletedAt = null;
                                note.DeletedBy = null;
                                note.DeletedExpiresAt = null;
                                note.UpdatedAt = DateTime.UtcNow; // Обновление даты
                                context.SaveChanges();
                                
                                LoadTrashNotes();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось восстановить заметку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RestoreToFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNote != null && _folders != null && _folders.Count > 0)
            {
                // Диалог выбора папки
                var dialog = new Window()
                {
                    Title = "Выберите папку для восстановления",
                    Width = 300,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };
                
                var stackPanel = new StackPanel()
                {
                    Margin = new Thickness(10)
                };
                
                var textBlock = new TextBlock()
                {
                    Text = "Выберите папку для восстановления заметки:",
                    Margin = new Thickness(0, 0, 0, 10)
                };
                
                var comboBox = new ComboBox()
                {
                    ItemsSource = _folders,
                    DisplayMemberPath = "Name",
                    Margin = new Thickness(0, 0, 0, 10)
                };
                
                var buttonPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                
                var okButton = new Button()
                {
                    Content = "ОК",
                    Width = 75,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                
                var cancelButton = new Button()
                {
                    Content = "Отмена",
                    Width = 75
                };
                
                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);
                
                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(comboBox);
                stackPanel.Children.Add(buttonPanel);
                
                dialog.Content = stackPanel;
                
                bool dialogResult = false;
                okButton.Click += (s, args) => {
                    dialogResult = true;
                    dialog.Close();
                };
                
                cancelButton.Click += (s, args) => {
                    dialog.Close();
                };
                
                dialog.ShowDialog();
                
                if (dialogResult && comboBox.SelectedItem is Folder selectedFolder)
                {
                    var result = MessageBox.Show($"Вы уверены, что хотите восстановить '{_selectedNote.Title}' в папку '{selectedFolder.Name}'?", "Подтвердить восстановление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using (var context = new NotesAppContext())
                            {
                                var note = context.Notes.FirstOrDefault(n => n.Id == _selectedNote.Id);
                                if (note != null)
                                {
                                    note.DeletedAt = null;
                                    note.DeletedBy = null;
                                    note.DeletedExpiresAt = null;
                                    note.FolderId = selectedFolder.Id;
                                    note.UpdatedAt = DateTime.UtcNow; // Обновление даты
                                    context.SaveChanges();
                                    
                                    LoadTrashNotes();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Не удалось восстановить заметку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Нет доступных папок для восстановления.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeletePermanentlyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNote != null)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите навсегда удалить '{_selectedNote.Title}'? Это действие нельзя отменить.", "Подтвердить окончательное удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new NotesAppContext())
                        {
                            var note = context.Notes.FirstOrDefault(n => n.Id == _selectedNote.Id);
                            if (note != null)
                            {
                                context.Notes.Remove(note);
                                context.SaveChanges();
                                
                                LoadTrashNotes();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось окончательно удалить заметку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}