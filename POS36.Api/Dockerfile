# Dùng bộ công cụ .NET 9 để Build code
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["POS36.Api.csproj", "./"]
RUN dotnet restore "./POS36.Api.csproj"
COPY . .
RUN dotnet publish "POS36.Api.csproj" -c Release -o /out /p:UseAppHost=false

# Dùng bộ .NET 9 siêu nhẹ để chạy web (tiết kiệm dung lượng)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /out .
ENTRYPOINT ["dotnet", "POS36.Api.dll"]