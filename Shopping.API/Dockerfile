FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Shopping.API/Shopping.API.csproj", "Shopping.API/"]
RUN dotnet restore "Shopping.API/Shopping.API.csproj"

COPY . .
RUN dotnet build "Shopping.API/Shopping.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shopping.API/Shopping.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shopping.API.dll"]