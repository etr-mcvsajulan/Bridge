# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# Restore dependencies
RUN dotnet restore FriendlyRS1/FriendlyRS1.csproj

# Publish in Release mode
RUN dotnet publish FriendlyRS1/FriendlyRS1.csproj -c Release -o /app/out

# Use the smaller ASP.NET runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/out .

# Set the entry point
ENTRYPOINT ["dotnet", "FriendlyRS1.dll"]

# Bind to the Railway-assigned port
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE ${PORT}
