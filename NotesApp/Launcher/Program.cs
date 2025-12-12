using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NotesApp.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Получаем директорию, где находится лаунчер
                string? launcherDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                // Переходим в директорию NotesApp (родительская директория)
                string? notesAppDir = Path.GetDirectoryName(launcherDir);
                
                if (launcherDir == null || notesAppDir == null)
                {
                    Console.WriteLine("Ошибка: Не удалось определить пути к директориям.");
                    Console.WriteLine("Нажмите любую клавишу для выхода...");
                    Console.ReadKey();
                    return;
                }
                
                // Проверяем, существует ли NotesApp.csproj
                string projectPath = Path.Combine(notesAppDir, "NotesApp.csproj");
                if (!File.Exists(projectPath))
                {
                    Console.WriteLine("Ошибка: NotesApp.csproj не найден.");
                    Console.WriteLine("Пожалуйста, убедитесь, что лаунчер находится в правильной директории.");
                    Console.WriteLine("Нажмите любую клавишу для выхода...");
                    Console.ReadKey();
                    return;
                }
                
                // Запускаем Notes App с помощью dotnet run
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ".net",
                    Arguments = "run --project NotesApp.csproj",
                    WorkingDirectory = notesAppDir,
                    UseShellExecute = false
                };
                
                Process? process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка запуска Notes App: {ex.Message}");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }
    }
}