# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY ["ExpenseTracker.slnx", "./"]
COPY ["src/ExpenseTracker.Domain/ExpenseTracker.Domain.csproj", "src/ExpenseTracker.Domain/"]
COPY ["src/ExpenseTracker.Application/ExpenseTracker.Application.csproj", "src/ExpenseTracker.Application/"]
COPY ["src/ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj", "src/ExpenseTracker.Infrastructure/"]
COPY ["src/ExpenseTracker.API/ExpenseTracker.API.csproj", "src/ExpenseTracker.API/"]

# Restore packages
RUN dotnet restore "src/ExpenseTracker.API/ExpenseTracker.API.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/src/ExpenseTracker.API"
RUN dotnet build "ExpenseTracker.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ExpenseTracker.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Use the built-in non-root user (available in .NET 8+)
USER $APP_UID

ENTRYPOINT ["dotnet", "ExpenseTracker.API.dll"]
