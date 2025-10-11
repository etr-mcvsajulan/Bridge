#!/bin/bash
# Restore, build, and run your ASP.NET Core app
dotnet restore FriendlyRS1/FriendlyRS1.csproj
dotnet publish FriendlyRS1/FriendlyRS1.csproj -c Release -o out
dotnet out/FriendlyRS1.dll --urls "http://0.0.0.0:${PORT:-8080}"
