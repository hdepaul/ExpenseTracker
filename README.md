# Expense Tracker API

[![CI - Build and Test](https://github.com/YOUR_USERNAME/ExpenseTracker/actions/workflows/ci.yml/badge.svg)](https://github.com/YOUR_USERNAME/ExpenseTracker/actions/workflows/ci.yml)
[![CD - Deploy to Azure](https://github.com/YOUR_USERNAME/ExpenseTracker/actions/workflows/cd.yml/badge.svg)](https://github.com/YOUR_USERNAME/ExpenseTracker/actions/workflows/cd.yml)

A RESTful API for personal expense tracking built with .NET 10, following Clean Architecture principles and best practices.

## Architecture

This project implements **Clean Architecture** with clear separation of concerns:

```
src/
├── ExpenseTracker.Domain        # Entities, Value Objects, Domain Events
├── ExpenseTracker.Application   # Use Cases, CQRS Handlers, Interfaces
├── ExpenseTracker.Infrastructure # EF Core, JWT, External Services
└── ExpenseTracker.API           # Controllers, Middleware, Configuration
```

### Key Patterns & Technologies

- **Clean Architecture** - Dependency inversion with clear layer boundaries
- **CQRS** with MediatR - Command/Query separation for better scalability
- **Repository Pattern** - Abstracted data access through interfaces
- **JWT Authentication** - Stateless authentication with refresh tokens
- **FluentValidation** - Declarative request validation
- **Entity Framework Core 10** - Code-first database with SQL Server
- **Docker** - Containerized deployment with docker-compose
- **GitHub Actions** - CI/CD pipelines for automated testing and deployment

## Features

- User registration and JWT-based authentication
- CRUD operations for expenses
- Category management (default + custom categories)
- Expense filtering by date range and category
- Pagination support for large datasets
- Global exception handling
- Request validation pipeline
- Request/Response logging
- **36 unit tests** with xUnit, Moq, and FluentAssertions

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional)
- SQL Server (local or Docker)

### Running with Docker

The easiest way to run the application:

```bash
docker-compose up -d
```

This will start:
- **API** at `http://localhost:5000`
- **SQL Server** at `localhost:1433`
- **API Documentation** at `http://localhost:5000/scalar/v1`

### Running Locally

1. Update the connection string in `appsettings.Development.json`

2. Run the API:
```bash
cd src/ExpenseTracker.API
dotnet run
```

3. Access the API documentation at `https://localhost:5001/scalar/v1`

### Running Tests

```bash
dotnet test
```

### Database Migrations

Create a new migration:
```bash
cd src/ExpenseTracker.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../ExpenseTracker.API
```

Update database:
```bash
dotnet ef database update --startup-project ../ExpenseTracker.API
```

## CI/CD Pipeline

This project uses **GitHub Actions** for continuous integration and deployment.

### Pipeline Overview

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   git push  │ ──► │    Build    │ ──► │    Test     │ ──► │   Deploy    │
│             │     │             │     │  (36 tests) │     │  to Azure   │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

### Workflows

| Workflow | Trigger | What it does |
|----------|---------|--------------|
| **CI** | Push, Pull Request | Build + Run Tests |
| **CD** | Push to main | Build + Test + Deploy to Azure |

### Setting up Azure Deployment

1. Create Azure resources:
   ```bash
   # Create Resource Group
   az group create --name expense-tracker-rg --location eastus

   # Create App Service Plan
   az appservice plan create --name expense-tracker-plan \
     --resource-group expense-tracker-rg --sku B1 --is-linux

   # Create Web App
   az webapp create --name expense-tracker-api \
     --resource-group expense-tracker-rg \
     --plan expense-tracker-plan \
     --deployment-container-image-name ghcr.io/YOUR_USERNAME/expense-tracker-api:latest
   ```

2. Create Azure credentials for GitHub:
   ```bash
   az ad sp create-for-rbac --name "expense-tracker-github" \
     --role contributor \
     --scopes /subscriptions/{subscription-id}/resourceGroups/expense-tracker-rg \
     --sdk-auth
   ```

3. Add the output as a GitHub Secret named `AZURE_CREDENTIALS`

4. Update the workflow file with your Azure Web App name

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login and get JWT token |

### Expenses

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/expenses` | Get all expenses (paginated) |
| GET | `/api/expenses/{id}` | Get expense by ID |
| POST | `/api/expenses` | Create new expense |
| PUT | `/api/expenses/{id}` | Update expense |
| DELETE | `/api/expenses/{id}` | Delete expense |

### Categories

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/categories` | Get all categories |

## Project Structure

```
ExpenseTracker/
├── .github/
│   └── workflows/
│       ├── ci.yml              # CI pipeline (build + test)
│       └── cd.yml              # CD pipeline (deploy to Azure)
├── src/
│   ├── ExpenseTracker.Domain/
│   │   ├── Common/             # BaseEntity, Value Objects
│   │   ├── Entities/           # User, Expense, Category
│   │   ├── Enums/              # ExpenseType
│   │   └── Exceptions/         # Domain exceptions
│   │
│   ├── ExpenseTracker.Application/
│   │   ├── Common/
│   │   │   ├── Behaviors/      # Validation, Logging pipelines
│   │   │   ├── Interfaces/     # IDbContext, IJwtService
│   │   │   └── Models/         # Result, PagedResult
│   │   └── Features/
│   │       ├── Auth/           # Register, Login commands
│   │       ├── Expenses/       # CRUD commands/queries
│   │       └── Categories/     # Category queries
│   │
│   ├── ExpenseTracker.Infrastructure/
│   │   ├── Data/               # DbContext, Configurations, Migrations
│   │   └── Services/           # JWT, PasswordHasher
│   │
│   └── ExpenseTracker.API/
│       ├── Controllers/        # API endpoints
│       └── Middleware/         # Exception handling
│
├── tests/
│   └── ExpenseTracker.Application.Tests/
│       ├── Domain/             # Entity tests
│       └── Features/           # Handler and Validator tests
│
├── docker-compose.yml
├── Dockerfile
└── README.md
```

## Configuration

Key configuration in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ExpenseTrackerDb;..."
  },
  "Jwt": {
    "Key": "YourSecretKeyHere",
    "Issuer": "ExpenseTrackerAPI",
    "Audience": "ExpenseTrackerClient",
    "ExpirationMinutes": 60
  }
}
```

## Security

- Passwords are hashed using PBKDF2 with SHA256
- JWT tokens with configurable expiration
- User-scoped data access (users can only access their own expenses)
- Input validation on all endpoints
- SQL injection prevention via parameterized queries (EF Core)

## License

This project is licensed under the MIT License.
