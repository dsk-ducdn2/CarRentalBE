# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /src

# Copy the project file and restore dependencies
COPY CarRental/CarRental.csproj CarRental/
RUN dotnet restore CarRental/CarRental.csproj

# Copy the rest of the application code
COPY . .

# Build the application
WORKDIR /src/CarRental
RUN dotnet build CarRental.csproj -c Release -o /app/build

# Publish the application
RUN dotnet publish CarRental.csproj -c Release -o /app/publish

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the ports
EXPOSE 5000 5001

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000;https://+:5001
ENV ASPNETCORE_ENVIRONMENT=Development

# Start the application
ENTRYPOINT ["dotnet", "CarRental.dll"]