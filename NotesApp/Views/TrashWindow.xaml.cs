using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NotesApp.Data;
using NotesApp.Models;

namespace NotesApp.Views
{
    public partial class TrashWindow : Window
    {
        private User _currentUser;
        private List<Note> _trashNotes;

        public TrashWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadTrashNotes();
        }

        private void LoadTrashNotes()
        {
            try
            {
                using (var context = new NotesAppContext())
                {
                    _trashNotes = context.Notes
                        .Where(n => n.AuthorId == _currentUser.Id && n.DeletedAt != null)
                        .ToList();
                    
                    TrashDataGrid.ItemsSource = _trashNotes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить корзину: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrashDataGrid.SelectedItem is Note selectedNote)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите восстановить '{selectedNote.Title}'?", "Подтвердить восстановление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new NotesAppContext())
                        {
                            var note = context.Notes.FirstOrDefault(n => n.Id == selectedNote.Id);
                            if (note != null)
                            {
                                note.DeletedAt = null;
                                note.DeletedBy = null;
                                note.DeletedExpiresAt = null;
                                context.SaveChanges();
                                
                                LoadTrashNotes();
                                
                                // Апдейтим кнопки
                                RestoreButton.IsEnabled = false;
                                DeletePermanentlyButton.IsEnabled = false;
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

        private void DeletePermanentlyButton_Click(object sender, RoutedEventArgs e)
        {
            if (TrashDataGrid.SelectedItem is Note selectedNote)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите навсегда удалить '{selectedNote.Title}'? Это действие нельзя отменить.", "Подтвердить окончательное удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new NotesAppContext())
                        {
                            var note = context.Notes.FirstOrDefault(n => n.Id == selectedNote.Id);
                            if (note != null)
                            {
                                context.Notes.Remove(note);
                                context.SaveChanges();
                                
                                LoadTrashNotes();
                                
                                // Апдейтим кнопки
                                RestoreButton.IsEnabled = false;
                                DeletePermanentlyButton.IsEnabled = false;
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

        private void TrashDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = TrashDataGrid.SelectedItem != null;
            RestoreButton.IsEnabled = hasSelection;
            DeletePermanentlyButton.IsEnabled = hasSelection;
        }
    }
}