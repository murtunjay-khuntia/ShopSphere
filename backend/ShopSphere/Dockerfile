FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ShopSphere.Domain/ShopSphere.Domain.csproj", "ShopSphere.Domain/"]
COPY ["ShopSphere.Application/ShopSphere.Application.csproj", "ShopSphere.Application/"]
COPY ["ShopSphere.Infrastructure/ShopSphere.Infrastructure.csproj", "ShopSphere.Infrastructure/"]
COPY ["ShopSphere.API/ShopSphere.API.csproj", "ShopSphere.API/"]
RUN dotnet restore "ShopSphere.API/ShopSphere.API.csproj"
COPY . .
WORKDIR "/src/ShopSphere.API"
RUN dotnet publish "ShopSphere.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShopSphere.API.dll"]
