# Use the official .NET 8.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy everything and restore as distinct layers
COPY . ./
RUN dotnet restore "./FoosballProLeague.Webserver/FoosballProLeague.Webserver.csproj"

# Build the application
RUN dotnet publish "./FoosballProLeague.Webserver/FoosballProLeague.Webserver.csproj" -c Release -o out

# Use the .NET 8.0 runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose port 5000 for the webserver
EXPOSE 5000

# Set the environment variable for the port
ENV ASPNETCORE_URLS=http://+:5000

# Set the entry point to run the Webserver
ENTRYPOINT ["dotnet", "FoosballProLeague.Webserver.dll"]
