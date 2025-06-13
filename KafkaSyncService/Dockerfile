FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["KafkaSyncService/EWP.SF.KafkaSync.API/EWP.SF.KafkaSync.API.csproj", "KafkaSyncService/EWP.SF.KafkaSync.API/"]
COPY ["KafkaSyncService/EWP.SF.KafkaSync.BusinessEntities/EWP.SF.KafkaSync.BusinessEntities.csproj", "KafkaSyncService/EWP.SF.KafkaSync.BusinessEntities/"]
COPY ["KafkaSyncService/EWP.SF.KafkaSync.BusinessLayer/EWP.SF.KafkaSync.BusinessLayer.csproj", "KafkaSyncService/EWP.SF.KafkaSync.BusinessLayer/"]
COPY ["KafkaSyncService/EWP.SF.KafkaSync.DataAccess/EWP.SF.KafkaSync.DataAccess.csproj", "KafkaSyncService/EWP.SF.KafkaSync.DataAccess/"]
COPY ["SharedService/EWP.SF.Common/EWP.SF.Common.csproj", "SharedService/EWP.SF.Common/"]
COPY ["SharedService/EWP.SF.Helper/EWP.SF.Helper.csproj", "SharedService/EWP.SF.Helper/"]

# Restore dependencies
RUN dotnet restore "KafkaSyncService/EWP.SF.KafkaSync.API/EWP.SF.KafkaSync.API.csproj"

# Copy the entire solution
COPY . .

# Create Lib directory and copy required DLLs
RUN mkdir -p /src/Lib/Windows
COPY ["Lib/Windows/EWP.SF.ConnectionModule.dll", "/src/Lib/Windows/"]

# Copy Settings directory
RUN mkdir -p /src/Settings
COPY ["KafkaSyncService/Settings/", "/src/Settings/"]

# Replace Program.cs with Docker-specific version
WORKDIR "/src/KafkaSyncService/EWP.SF.KafkaSync.API"
RUN mv Program.Docker.cs Program.cs
RUN mv appsettings.Docker.json appsettings.json

# Build the project
RUN dotnet build "EWP.SF.KafkaSync.API.csproj" -c Debug -o /app/build

FROM build AS publish
# Add this to preserve source info for debugging
RUN dotnet publish "EWP.SF.KafkaSync.API.csproj" -c Debug -o /app/publish /p:UseAppHost=false /p:DebugType=portable /p:DebugSymbols=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories for settings and lib
RUN mkdir -p /app/Lib/Windows
RUN mkdir -p /app/Settings

# Copy the ConnectionModule DLL
COPY --from=build /src/Lib/Windows/EWP.SF.ConnectionModule.dll /app/Lib/Windows/

# Copy Settings files
COPY --from=build /src/Settings/ /app/Settings/

# Copy appsettings.json
COPY --from=build /src/KafkaSyncService/EWP.SF.KafkaSync.API/appsettings.json /app/appsettings.json

# Install the VS remote debugger
RUN apt-get update \
    && apt-get install -y --no-install-recommends unzip procps curl \
    && rm -rf /var/lib/apt/lists/* \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

ENTRYPOINT ["dotnet", "EWP.SF.KafkaSync.API.dll"]
