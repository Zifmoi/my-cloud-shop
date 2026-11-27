# ЭТАП 1: Сборка и компиляция проекта (используем полный SDK .NET 10.0)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Сначала копируем только файл проекта и восстанавливаем зависимости (ускоряет повторные сборки за счёт кэша)
COPY ["CloudShop.csproj", "./"]
RUN dotnet restore

# Копируем весь остальной исходный код и публикуем релизную сборку в папку /app/publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# ЭТАП 2: Формирование лёгкого боевого образа (используем только среду выполнения ASP.NET 10.0)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Забираем скомпилированные файлы с первого этапа
COPY --from=build /app/publish .

# Открываем 80-й порт внутри контейнера и настраиваем веб-сервер Kestrel на его прослушивание
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Точка входа — команда, которая выполнится при старте контейнера
ENTRYPOINT ["dotnet", "CloudShop.dll"]