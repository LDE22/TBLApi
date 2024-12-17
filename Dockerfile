# Установка .NET SDK 7.0
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Копируем файлы проекта
COPY *.csproj ./
RUN dotnet restore

# Копируем все исходники и собираем проект
COPY . ./
RUN dotnet publish -c Release -o out

# Использование ASP.NET Core Runtime для запуска приложения
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Запуск API
ENTRYPOINT ["dotnet", "TBLApi.dll"]
