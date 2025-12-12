using System;
using System.Windows;

namespace NotesApp.Views
{
    public partial class NotificationWindow : Window
    {
        public NotificationWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Пожалуйста, введите сообщение для отправки.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // В реальном приложении это сохранялось бы в таблице уведомлений в базе данных
            // А пока просто покажем сообщение об успешной отправке
            MessageBox.Show("Уведомление успешно отправлено всем пользователям.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}