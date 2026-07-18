# Build context must be srcs/app/ (not srcs/app/api/) because the API references sibling projects.
# Run from repo root: podman build -f srcs/app/api/Dockerfile srcs/app/ -t storage-api
#
# Base images are Linux (Debian-based). No Windows containers are used.

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching on restore
COPY api/storageapi.csproj api/
COPY dal/storagedal/storagedal.csproj dal/storagedal/
COPY sharedentities/sharedentities/sharedentities.csproj sharedentities/sharedentities/
RUN dotnet restore api/storageapi.csproj

# Copy source and publish
COPY api/ api/
COPY dal/ dal/
COPY sharedentities/ sharedentities/
RUN dotnet publish api/storageapi.csproj -c Release -o /app/publish

# Stage 2: Runtime — Ubuntu 24.04 (noble). Longer support (Apr 2029) and newer OpenSSL 3.2 vs Debian default.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "storageapi.dll"]
