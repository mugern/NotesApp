# Скрипт сборки Notes App
# Этот скрипт соберет все решение и создаст пакет развертывания

# Проверяем, находимся ли мы в правильной директории
if (-not (Test-Path "NotesApp.sln")) {
    Write-Host "Ошибка: NotesApp.sln не найден. Пожалуйста, запустите этот скрипт из корневой директории." -ForegroundColor Red
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

Write-Host "Сборка решения Notes App..." -ForegroundColor Green

# Очищаем предыдущие сборки
Write-Host "Очистка предыдущих сборок..." -ForegroundColor Yellow
dotnet clean

# Восстанавливаем пакеты NuGet
Write-Host "Восстановление пакетов NuGet..." -ForegroundColor Yellow
dotnet restore

# Собираем решение
Write-Host "Сборка решения..." -ForegroundColor Yellow
dotnet build --configuration Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "Сборка успешна." -ForegroundColor Green
    
    # Публикуем основное приложение
    Write-Host "Публикация NotesApp..." -ForegroundColor Yellow
    dotnet publish NotesApp/NotesApp.csproj --configuration Release --output ./publish/NotesApp
    
    # Публикуем лаунчер
    Write-Host "Публикация лаунчера..." -ForegroundColor Yellow
    dotnet publish NotesApp/Launcher/Launcher.csproj --configuration Release --output ./publish/Launcher
    
    Write-Host "Сборка и публикация успешно завершены!" -ForegroundColor Green
    Write-Host "Пакет развертывания создан в директории ./publish/" -ForegroundColor Green
} else {
    Write-Host "Сборка не удалась. Пожалуйста, проверьте сообщения об ошибках выше." -ForegroundColor Red
}

pause