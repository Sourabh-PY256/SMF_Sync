FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ShopFloorService/EWP.SF.ShopFloor.API/EWP.SF.ShopFloor.API.csproj", "ShopFloorService/EWP.SF.ShopFloor.API/"]
COPY ["ShopFloorService/EWP.SF.ShopFloor.BusinessEntities/EWP.SF.ShopFloor.BusinessEntities.csproj", "ShopFloorService/EWP.SF.ShopFloor.BusinessEntities/"]
COPY ["ShopFloorService/EWP.SF.ShopFloor.BusinessLayer/EWP.SF.ShopFloor.BusinessLayer.csproj", "ShopFloorService/EWP.SF.ShopFloor.BusinessLayer/"]
COPY ["ShopFloorService/EWP.SF.ShopFloor.DataAccess/EWP.SF.ShopFloor.DataAccess.csproj", "ShopFloorService/EWP.SF.ShopFloor.DataAccess/"]
COPY ["SharedService/EWP.SF.Common/EWP.SF.Common.csproj", "SharedService/EWP.SF.Common/"]
COPY ["SharedService/EWP.SF.Helper/EWP.SF.Helper.csproj", "SharedService/EWP.SF.Helper/"]

# Restore dependencies
RUN dotnet restore "ShopFloorService/EWP.SF.ShopFloor.API/EWP.SF.ShopFloor.API.csproj"

# Copy the entire solution
COPY . .

# Create Lib directory and copy required DLLs
RUN mkdir -p /src/Lib/Windows
COPY ["Lib/Windows/EWP.SF.ConnectionModule.dll", "/src/Lib/Windows/"]

# Copy Settings directory
RUN mkdir -p /src/Settings
COPY ["ShopFloorService/Settings/", "/src/Settings/"]

# Replace Program.cs with Docker-specific version
WORKDIR "/src/ShopFloorService/EWP.SF.ShopFloor.API"
RUN mv Program.Docker.cs Program.cs
RUN mv appsettings.Docker.json appsettings.json

# Build the project
RUN dotnet build "EWP.SF.ShopFloor.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EWP.SF.ShopFloor.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
COPY --from=build /src/ShopFloorService/EWP.SF.ShopFloor.API/appsettings.json /app/appsettings.json

ENTRYPOINT ["dotnet", "EWP.SF.ShopFloor.API.dll"]
