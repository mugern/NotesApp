# Скрипт запуска Notes App
# Этот скрипт соберет и запустит Notes App

# Проверяем, находимся ли мы в правильной директории
if (-not (Test-Path "NotesApp.csproj")) {
    Write-Host "Ошибка: NotesApp.csproj не найден. Пожалуйста, запустите этот скрипт из директории NotesApp." -ForegroundColor Red
    pause
    exit 1
}

# Проверяем, установлен ли .NET 6.0
try {
    $dotnetVersion = dotnet --version 2>$null
    if (-not $dotnetVersion) {
        Write-Host "Ошибка: .NET SDK не найден. Пожалуйста, установите .NET 6.0 или новее." -ForegroundColor Red
        pause
        exit 1
    }
} catch {
    Write-Host "Ошибка: .NET SDK не найден. Пожалуйста, установите .NET 6.0 или новее." -ForegroundColor Red
    pause
    exit 1
}

Write-Host "Сборка Notes App..." -ForegroundColor Green

# Собираем проект
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Сборка успешна. Запуск Notes App..." -ForegroundColor Green
    dotnet run
} else {
    Write-Host "Сборка не удалась. Пожалуйста, проверьте сообщения об ошибках выше." -ForegroundColor Red
}

pause