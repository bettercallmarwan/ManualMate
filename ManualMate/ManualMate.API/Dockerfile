# Stage 1: Base Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Stage 2: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files for restore cache optimization
COPY ["ManualMate.API/ManualMate.csproj", "ManualMate.API/"]
COPY ["ManualMate.Application/ManualMate.Application.csproj", "ManualMate.Application/"]
COPY ["ManualMate.Domain/ManualMate.Domain.csproj", "ManualMate.Domain/"]
COPY ["ManualMate.Infrastructure/ManualMate.Infrastructure.csproj", "ManualMate.Infrastructure/"]

RUN dotnet restore "ManualMate.API/ManualMate.csproj"

# Copy remaining source code
COPY . .

WORKDIR "/src/ManualMate.API"
RUN dotnet publish "ManualMate.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Create directory for file uploads if it doesn't exist
RUN mkdir -p wwwroot/Files

ENTRYPOINT ["dotnet", "ManualMate.dll"]
