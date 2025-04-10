# Stage 1 - build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# Restore dependencies
RUN dotnet restore POSIMSWebApi/POSIMSWebApi.csproj

# Build the app
RUN dotnet publish POSIMSWebApi/POSIMSWebApi.csproj -c Release -o out

# Stage 2 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Set entry point
ENTRYPOINT ["dotnet", "POSIMSWebApi.dll"]