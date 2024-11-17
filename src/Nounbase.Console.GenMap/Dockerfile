# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /dockersrc
COPY ["Nounbase.Console.GenMap/Nounbase.Console.GenMap.csproj", "Nounbase.Console.GenMap/"]
COPY ["../Nounbase.Core/Nounbase.Core.csproj", "Nounbase.Core/"]
COPY ["../Nounbase.Services.SqlServer/Nounbase.Services.SqlServer.csproj", "Nounbase.Services.SqlServer/"]
COPY ["../Nounbase.Services/Nounbase.Services.csproj", "Nounbase.Services/"]
RUN dotnet restore "./Nounbase.Console.GenMap/Nounbase.Console.GenMap.csproj"
COPY . .
WORKDIR "/dockersrc/Nounbase.Console.GenMap"
RUN dotnet build "./Nounbase.Console.GenMap.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Nounbase.Console.GenMap.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nounbase.Console.GenMap.dll"]
