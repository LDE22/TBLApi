# ��������� .NET SDK 7.0
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# �������� ����� �������
COPY *.csproj ./
RUN dotnet restore

# �������� ��� ��������� � �������� ������
COPY . ./
RUN dotnet publish -c Release -o out

# ������������� ASP.NET Core Runtime ��� ������� ����������
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# ������ API
ENTRYPOINT ["dotnet", "TBLApi.dll"]
