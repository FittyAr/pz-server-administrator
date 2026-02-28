# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["pz-server-administrator.csproj", "./"]
RUN dotnet restore "pz-server-administrator.csproj"

# Copy the rest of the code
COPY . .
RUN dotnet build "pz-server-administrator.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "pz-server-administrator.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

# Create config directory and ensure permissions
RUN mkdir -p /app/config && chmod 777 /app/config

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "pz-server-administrator.dll"]
