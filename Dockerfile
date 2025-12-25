# --- 1. Aşama: İnşaat (Build) ---
# Burası projeyi derleyen kısımdır.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyanın adı neyse buraya onu yaz (Örn: LibrarySystem.csproj)
COPY ["LibrarySystem.csproj", "./"]
RUN dotnet restore

# Kalan tüm dosyaları kopyala ve yayınla
COPY . .
RUN dotnet publish -c Release -o /app/publish

# --- 2. Aşama: Çalıştırma (Run) ---
# Burası bitmiş projeyi sunan kısımdır.
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render'ın verdiği portu dinlemesi için gerekli ayar
ENV ASPNETCORE_URLS=http://+:80

# Başlatılacak ana dosya (Proje adınla aynı olmalı)
ENTRYPOINT ["dotnet", "LibrarySystem.dll"]