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
                        .Include(n => n.Folder) // Включаем данные папки
                        .Where(n => n.AuthorId == _currentUser.Id && n.DeletedAt != null)
                        .OrderByDescending(n => n.DeletedAt) // Сортируем по дате удаления
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
            // Очищаем панель перед добавлением новых элементов
            TrashNotesPanel.Children.Clear();

            // Создаем элементы для каждой удаленной заметки
            foreach (var note in _trashNotes)
            {
                // Создаем контейнер для заметки
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

                // Создаем панель для содержимого заметки
                StackPanel notePanel = new StackPanel
                {
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Создаем текстовое поле для заголовка заметки
                TextBlock titleTextBlock = new TextBlock
                {
                    Text = note.Title,
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 5),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.Black)
                };

                // Создаем текстовое поле для папки
                TextBlock folderTextBlock = new TextBlock
                {
                    Text = note.Folder?.Name ?? "Без папки",
                    FontStyle = FontStyles.Italic,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Colors.Black),
                    Margin = new Thickness(0, 0, 0, 5),
                    TextWrapping = TextWrapping.Wrap
                };

                // Создаем текстовое поле для даты удаления
                TextBlock deletedTextBlock = new TextBlock
                {
                    Text = note.DeletedAt.HasValue ?
                        $"Удалено: {note.DeletedAt.Value:dd.MM.yyyy HH:mm}" :
                        "Удалено: Не указано",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Black),
                    Margin = new Thickness(0, 0, 0, 3)
                };

                // Создаем текстовое поле для даты истечения
                TextBlock expiresTextBlock = new TextBlock
                {
                    Text = note.DeletedExpiresAt.HasValue ?
                        $"Истекает через: {Math.Max(0, (note.DeletedExpiresAt.Value - DateTime.UtcNow).Days)} дней" :
                        "Истекает: Не указано",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Black)
                };

                // Добавляем все элементы в панель
                notePanel.Children.Add(titleTextBlock);
                notePanel.Children.Add(folderTextBlock);
                notePanel.Children.Add(deletedTextBlock);
                notePanel.Children.Add(expiresTextBlock);

                // Добавляем панель в контейнер
                noteBorder.Child = notePanel;

                // Добавляем обработчик клика для выбора заметки
                noteBorder.MouseLeftButtonUp += (sender, e) => SelectNote(note, noteBorder);

                // Добавляем контейнер в панель заметок
                TrashNotesPanel.Children.Add(noteBorder);
            }

            // Сбрасываем выбор
            _selectedNote = null;
            RestoreButton.IsEnabled = false;
            RestoreToFolderButton.IsEnabled = false;
            DeletePermanentlyButton.IsEnabled = false;
        }

        private void SelectNote(Note note, Border border)
        {
            // Сбрасываем выделение для всех элементов
            foreach (UIElement element in TrashNotesPanel.Children)
            {
                if (element is Border b)
                {
                    b.BorderBrush = new SolidColorBrush(Colors.DarkRed);
                    b.BorderThickness = new Thickness(2);
                }
            }

            // Выделяем выбранную заметку
            border.BorderBrush = new SolidColorBrush(Colors.Green);
            border.BorderThickness = new Thickness(3);

            // Сохраняем выбранную заметку
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
                                note.UpdatedAt = DateTime.UtcNow; // Обновляем дату изменения
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
                // Создаем диалоговое окно для выбора папки
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
                                    note.UpdatedAt = DateTime.UtcNow; // Обновляем дату изменения
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