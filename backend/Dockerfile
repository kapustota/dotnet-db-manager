# Используем базовый образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /backend


# Копируем файл проекта и восстанавливаем зависимости
COPY *.csproj ./
RUN echo "Copying project file" && ls -la && dotnet restore && echo "Dependencies restored"

# Копируем остальные файлы и собираем проект
COPY . ./
RUN echo "Copying remaining files" && ls -la && dotnet build -c Release -o /backend/build && echo "Project built"

# Публикуем проект
RUN dotnet publish -c Release -o /backend/publish && echo "Project published"

# Используем базовый образ ASP.NET Core для запуска
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /backend

# Установка fontconfig и шрифтов в runtime стадии
RUN apt-get update && apt-get install -y --no-install-recommends fontconfig fonts-dejavu && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# Обновление кеша шрифтов
RUN fc-cache -fv

# Копируем опубликованные файлы из стадии сборки
COPY --from=build /backend/publish .

# Копируем скрипт wait-for-it.sh
COPY wait-for-it.sh /wait-for-it.sh
RUN chmod +x /wait-for-it.sh

# Настроим точку входа для запуска приложения с ожиданием PostgreSQL
ENTRYPOINT ["/wait-for-it.sh", "postgres_db:5432", "--", "dotnet", "Backend.dll"]