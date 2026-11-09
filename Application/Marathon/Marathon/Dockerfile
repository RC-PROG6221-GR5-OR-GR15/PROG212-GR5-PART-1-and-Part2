# See https://aka.ms/customizecontainer to learn how to customize your debug container.

# Runtime image used to run the published API.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build image: restore, compile and publish the Marathon API.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Marathon/Marathon.csproj", "Marathon/"]
RUN dotnet restore "./Marathon/Marathon.csproj"
COPY . .
WORKDIR "/src/Marathon"
RUN dotnet publish "./Marathon.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Marathon.dll"]
