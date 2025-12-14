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
    public partial class CreateFolderWindow : Window
    {
        private User _currentUser;
        private List<Note> _notes;
        private List<Note> _selectedNotes;
        
        public Folder CreatedFolder { get; private set; }

        public CreateFolderWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _selectedNotes = new List<Note>();
            LoadNotes();
        }

        private void LoadNotes()
        {
            try
            {
                using (var context = new NotesAppContext())
                {
                    _notes = context.Notes
                        .Include(n => n.Folder) // Включаем данные папки
                        .Where(n => n.AuthorId == _currentUser.Id && n.DeletedAt == null && n.IsArchived == false)
                        .ToList();
                    
                    DisplayNotes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Инициализируем пустой список заметок, чтобы приложение могло продолжить работу
                _notes = new List<Note>();
                DisplayNotes();
                // Продолжаем работу приложения, не прерывая его
            }
        }

        private void DisplayNotes()
        {
            // Очищаем панель перед добавлением новых элементов
            NotesPanel.Children.Clear();

            // Создаем элементы для каждой заметки
            foreach (var note in _notes)
            {
                // Создаем контейнер для заметки
                Border noteBorder = new Border
                {
                    Width = 180,
                    Height = 120,
                    Margin = new Thickness(5),
                    Background = new SolidColorBrush(Colors.LightBlue),
                    BorderBrush = new SolidColorBrush(Colors.DarkBlue),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3)
                };

                // Создаем панель для содержимого заметки
                StackPanel notePanel = new StackPanel
                {
                    Margin = new Thickness(5)
                };

                // Создаем текстовое поле для заголовка заметки
                TextBlock titleTextBlock = new TextBlock
                {
                    Text = note.Title,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 3),
                    TextWrapping = TextWrapping.Wrap
                };

                // Создаем текстовое поле для содержимого заметки
                TextBlock contentTextBlock = new TextBlock
                {
                    Text = note.Content.Length > 100 ? note.Content.Substring(0, 100) + "..." : note.Content,
                    FontSize = 10,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 3)
                };

                // Создаем текстовое поле для имени папки
                TextBlock folderTextBlock = new TextBlock
                {
                    Text = note.Folder?.Name ?? "Без папки",
                    FontStyle = FontStyles.Italic,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    Margin = new Thickness(0, 0, 0, 3)
                };

                // Создаем текстовое поле для даты создания
                TextBlock dateTextBlock = new TextBlock
                {
                    Text = $"Создано: {note.CreatedAt:dd.MM.yyyy}",
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Colors.Gray)
                };

                // Добавляем все элементы в панель
                notePanel.Children.Add(titleTextBlock);
                notePanel.Children.Add(contentTextBlock);
                notePanel.Children.Add(folderTextBlock);
                notePanel.Children.Add(dateTextBlock);

                // Добавляем панель в контейнер
                noteBorder.Child = notePanel;

                // Добавляем обработчик клика для выбора заметки
                noteBorder.MouseLeftButtonUp += (sender, e) => ToggleNoteSelection(note, noteBorder);

                // Добавляем контейнер в панель заметок
                NotesPanel.Children.Add(noteBorder);
            }
        }

        private void ToggleNoteSelection(Note note, Border border)
        {
            if (_selectedNotes.Contains(note))
            {
                // Убираем выделение
                _selectedNotes.Remove(note);
                border.Background = new SolidColorBrush(Colors.LightBlue);
                border.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
                border.BorderThickness = new Thickness(1);
            }
            else
            {
                // Добавляем выделение
                _selectedNotes.Add(note);
                border.Background = new SolidColorBrush(Colors.LightGreen);
                border.BorderBrush = new SolidColorBrush(Colors.DarkGreen);
                border.BorderThickness = new Thickness(2);
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string folderName = FolderNameTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(folderName))
            {
                MessageBox.Show("Пожалуйста, введите название папки.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                using (var context = new NotesAppContext())
                {
                    // Проверяем, существует ли уже папка с таким названием у текущего пользователя
                    var existingFolder = context.Folders.FirstOrDefault(f => f.Name == folderName && f.UserId == _currentUser.Id);
                    if (existingFolder != null)
                    {
                        MessageBox.Show("Папка с таким названием уже существует. Пожалуйста, выберите другое название.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Создаем новую папку
                    var folder = new Folder
                    {
                        Name = folderName,
                        UserId = _currentUser.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    context.Folders.Add(folder);
                    context.SaveChanges();
                    
                    // Обновляем выбранные заметки, чтобы они принадлежали новой папке
                    foreach (var note in _selectedNotes)
                    {
                        var noteToUpdate = context.Notes.FirstOrDefault(n => n.Id == note.Id);
                        if (noteToUpdate != null)
                        {
                            noteToUpdate.FolderId = folder.Id;
                        }
                    }
                    
                    context.SaveChanges();
                    CreatedFolder = folder;
                    
                    MessageBox.Show("Папка успешно создана.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось создать папку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Не закрываем окно, чтобы пользователь мог попробовать снова
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}