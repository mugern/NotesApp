using System;
using System.Linq;
using System.Windows;
using NotesApp.Data;
using NotesApp.Models;
using NotesApp.Utils;

namespace NotesApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, введите имя пользователя и пароль.");
                return;
            }

            try
            {
                using (var context = new NotesAppContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == username);
                    
                    if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash))
                    {
                        // Заходим!
                        MainWindow mainWindow = new MainWindow(user);
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        ShowError("Неверное имя пользователя или пароль.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка входа: {ex.Message}");
                // Продолжаем работу приложения, не прерывая его
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, введите имя пользователя и пароль.");
                return;
            }

            try
            {
                using (var context = new NotesAppContext())
                {
                    // Смотри, есть ли уже такой юзер
                    if (context.Users.Any(u => u.Username == username))
                    {
                        ShowError("Имя пользователя уже существует.");
                        return;
                    }

                    // Делаем нового юзера
                    var newUser = new User
                    {
                        Username = username,
                        Email = $"{username}@notesapp.com", // Дефолтная почта
                        PasswordHash = PasswordHasher.HashPassword(password)
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();

                    MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
                // Продолжаем работу приложения, не прерывая его
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}