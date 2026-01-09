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
                        .Include(n => n.Folder) // Данные папки
                        .Where(n => n.AuthorId == _currentUser.Id && n.DeletedAt == null && n.IsArchived == false)
                        .ToList();
                    
                    DisplayNotes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Пустой список заметок
                _notes = new List<Note>();
                DisplayNotes();
                // Продолжение работы
            }
        }

        private void DisplayNotes()
        {
            // Очистка панели
            NotesPanel.Children.Clear();
            
            // Создание элементов заметок
            foreach (var note in _notes)
            {
                // Контейнер заметки
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
                
                // Панель содержимого
                StackPanel notePanel = new StackPanel
                {
                    Margin = new Thickness(5)
                };
                
                // Текстовое поле заголовка
                TextBlock titleTextBlock = new TextBlock
                {
                    Text = note.Title,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 3),
                    TextWrapping = TextWrapping.Wrap
                };
                
                // Текстовое поле содержимого
                TextBlock contentTextBlock = new TextBlock
                {
                    Text = note.Content.Length > 100 ? note.Content.Substring(0, 100) + "..." : note.Content,
                    FontSize = 10,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 3)
                };
                
                // Текстовое поле папки
                TextBlock folderTextBlock = new TextBlock
                {
                    Text = note.Folder?.Name ?? "Без папки",
                    FontStyle = FontStyles.Italic,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    Margin = new Thickness(0, 0, 0, 3)
                };
                
                // Текстовое поле даты
                TextBlock dateTextBlock = new TextBlock
                {
                    Text = $"Создано: {note.CreatedAt:dd.MM.yyyy}",
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Colors.Gray)
                };
                
                // Добавление элементов
                notePanel.Children.Add(titleTextBlock);
                notePanel.Children.Add(contentTextBlock);
                notePanel.Children.Add(folderTextBlock);
                notePanel.Children.Add(dateTextBlock);
                
                // Добавление панели в контейнер
                noteBorder.Child = notePanel;
                
                // Обработчик клика
                noteBorder.MouseLeftButtonUp += (sender, e) => ToggleNoteSelection(note, noteBorder);
                
                // Добавление в панель
                NotesPanel.Children.Add(noteBorder);
            }
        }

        private void ToggleNoteSelection(Note note, Border border)
        {
            if (_selectedNotes.Contains(note))
            {
                // Снятие выделения
                _selectedNotes.Remove(note);
                border.Background = new SolidColorBrush(Colors.LightBlue);
                border.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
                border.BorderThickness = new Thickness(1);
            }
            else
            {
                // Добавление выделения
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
                    // Проверка существующей папки
                    var existingFolder = context.Folders.FirstOrDefault(f => f.Name == folderName && f.UserId == _currentUser.Id);
                    if (existingFolder != null)
                    {
                        MessageBox.Show("Папка с таким названием уже существует. Пожалуйста, выберите другое название.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Создание новой папки
                    var folder = new Folder
                    {
                        Name = folderName,
                        UserId = _currentUser.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    context.Folders.Add(folder);
                    context.SaveChanges();
                    
                    // Обновление выбранных заметок
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
                // Не закрываем окно
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}