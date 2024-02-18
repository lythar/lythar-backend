#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LytharBackend/LytharBackend.csproj", "LytharBackend/"]
RUN dotnet restore "./LytharBackend/./LytharBackend.csproj"
COPY . .
WORKDIR "/src/LytharBackend"
RUN dotnet build "./LytharBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LytharBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
RUN apt-get update \
  && apt-get install -y --no-install-recommends \
  libldap-2.5-0 \
  && rm -rf /var/lib/apt/lists/*
USER app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LytharBackend.dll"]