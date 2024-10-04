# Use the official .NET 8.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy everything and restore as distinct layers
COPY . ./
RUN dotnet restore "./Foosify.Api/Foosify.Api.csproj"

# Build the application
RUN dotnet publish "./Foosify.Api/Foosify.Api.csproj" -c Release -o out

# Use the .NET 8.0 runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose port 80 for the API
EXPOSE 5197

# Set the entry point to run the API
ENTRYPOINT ["dotnet", "Foosify.Api.dll"]
