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
            
            // Ставим светлую тему по умолчанию, как просил босс
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(Theme.Light);
            paletteHelper.SetTheme(theme);
            
            // Инициализируем базу данных, надеемся что не упадет
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
                // Показываем окно входа, даже если база данных не инициализирована
                var loginWindow = new Views.LoginWindow();
                loginWindow.Show();
                // Закрываем приложение, если база данных не может быть инициализирована
                Shutdown();
            }
            
            // Очищаем истекшие заметки
            try
            {
                int deletedCount = NoteCleanupService.CleanupNotes();
                if (deletedCount > 0)
                {
                    // Можно добавить логирование или уведомление о количестве удаленных заметок
                }
            }
            catch (Exception ex)
            {
                // Просто логируем ошибку, но не прерываем запуск приложения
                // В реальном приложении здесь должно быть логирование
                System.Diagnostics.Debug.WriteLine($"Ошибка при очистке заметок: {ex.Message}");
                // Продолжаем работу приложения, не прерывая его
            }
        }
    }
}