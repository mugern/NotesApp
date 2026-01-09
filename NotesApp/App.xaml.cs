using System;
using System.Windows;
using NotesApp.Data;
using NotesApp.Utils;
using MaterialDesignThemes.Wpf;

namespace NotesApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Светлая тема по умолчанию
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(Theme.Light);
            paletteHelper.SetTheme(theme);
            
            // Инициализация базы данных
            try
            {
                using (var context = new NotesAppContext())
                {
                    context.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось инициализировать базу данных: {ex.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                // Показ окна входа
                var loginWindow = new Views.LoginWindow();
                loginWindow.Show();
                // Закрытие приложения
                Shutdown();
            }
            
            // Очистка истекших заметок
            try
            {
                int deletedCount = NoteCleanupService.CleanupNotes();
                if (deletedCount > 0)
                {
                    
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                System.Diagnostics.Debug.WriteLine($"Ошибка при очистке заметок: {ex.Message}");
                // Продолжение работы
            }
        }
    }
}