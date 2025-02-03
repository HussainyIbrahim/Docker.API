# 1️ Use official .NET 6 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# 2️ Copy project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# 3️ Copy remaining files and build the project
COPY . ./
RUN dotnet publish -c Release -o out

# 4️ Use a smaller runtime image for final container
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .

# 5️ Expose port and start application
EXPOSE 80
ENTRYPOINT ["dotnet", "Docker.API.dll"]