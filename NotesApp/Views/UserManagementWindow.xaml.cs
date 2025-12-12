using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NotesApp.Data;
using NotesApp.Models;

namespace NotesApp.Views
{
    public partial class UserManagementWindow : Window
    {
        private List<User> _users;

        public UserManagementWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                using (var context = new NotesAppContext())
                {
                    _users = context.Users.ToList();
                    UsersDataGrid.ItemsSource = _users;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MakeAdminButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                if (selectedUser.IsAdmin)
                {
                    MessageBox.Show("Пользователь уже является администратором.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show($"Вы уверены, что хотите сделать '{selectedUser.Username}' администратором?", "Подтвердить повышение администратора", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new NotesAppContext())
                        {
                            var user = context.Users.FirstOrDefault(u => u.Id == selectedUser.Id);
                            if (user != null)
                            {
                                user.IsAdmin = true;
                                context.SaveChanges();
                                
                                LoadUsers();
                                
                                MessageBox.Show($"Пользователь '{user.Username}' теперь администратор.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось повысить пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RemoveAdminButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                if (!selectedUser.IsAdmin)
                {
                    MessageBox.Show("Пользователь не является администратором.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Не даем убрать админку у себя самого
                // Нужно бы передать права другому, но пока просто предупреждаем
                var result = MessageBox.Show($"Вы уверены, что хотите удалить права администратора у '{selectedUser.Username}'?", "Подтвердить удаление администратора", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new NotesAppContext())
                        {
                            var user = context.Users.FirstOrDefault(u => u.Id == selectedUser.Id);
                            if (user != null)
                            {
                                user.IsAdmin = false;
                                context.SaveChanges();
                                
                                LoadUsers();
                                
                                MessageBox.Show($"Пользователь '{user.Username}' больше не администратор.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось удалить права администратора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{selectedUser.Username}'? Это также удалит все его папки и заметки.", "Подтвердить удаление пользователя", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new NotesAppContext())
                        {
                            var user = context.Users.FirstOrDefault(u => u.Id == selectedUser.Id);
                            if (user != null)
                            {
                                context.Users.Remove(user);
                                context.SaveChanges();
                                
                                LoadUsers();
                                
                                MessageBox.Show($"Пользователь '{user.Username}' был удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось удалить пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = UsersDataGrid.SelectedItem != null;
            MakeAdminButton.IsEnabled = hasSelection;
            RemoveAdminButton.IsEnabled = hasSelection;
            DeleteUserButton.IsEnabled = hasSelection;
        }
    }
}