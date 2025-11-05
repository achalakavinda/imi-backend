# MigratingAssistant

A comprehensive immigration support platform built with Clean Architecture, providing services for immigration assistance, job applications, document management, bookings, and payments.

## Table of Contents

- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Configuration](#configuration)
  - [Database Setup](#database-setup)
  - [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Authentication](#authentication)
- [Exposed Entities](#exposed-entities)
- [Development](#development)
- [Testing](#testing)

## Overview

The MigratingAssistant platform is a clean architecture solution that provides:

- **User Management** with role-based authorization (Guest, User, Administrator)
- **JWT Authentication** with refresh token rotation
- **Service Marketplace** for immigration service providers
- **Job Board** for immigration-related employment
- **Booking System** for appointments and services
- **Document Management** for immigration paperwork
- **Payment Processing** integration
- **Support Ticketing** system
- **Audit Logging** for compliance

## Technology Stack

- **.NET 8.0** - Target framework
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core 8.0** - ORM
- **MySQL 8.0+** - Database (via Pomelo.EntityFrameworkCore.MySql)
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Request validation
- **AutoMapper** - Object mapping
- **JWT Bearer Authentication** - Token-based authentication
- **xUnit** - Unit testing framework

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL 8.0 or higher](https://dev.mysql.com/downloads/mysql/)
- (Optional) [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Configuration

#### 1. Database Connection String

Update the connection string in `src/Web/appsettings.json` and `src/Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MigratingAssistant;User=root;Password=your_password;"
  }
}
```

#### 2. JWT Settings

Configure JWT authentication in `src/Web/appsettings.json`:

```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-min-32-characters-long-for-security",
    "Issuer": "MigratingAssistant",
    "Audience": "MigratingAssistantUsers",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  }
}
```

**Important:** The `Secret` must be at least 32 characters long. For development, a default secret is provided in `appsettings.Development.json`. **Always change this for production!**

#### 3. Identity Settings

Configure password requirements in `src/Infrastructure/DependencyInjection.cs` (lines 32-40):

```csharp
options.Password.RequireDigit = true;
options.Password.RequireLowercase = true;
options.Password.RequireNonAlphanumeric = true;
options.Password.RequireUppercase = true;
options.Password.RequiredLength = 6;
options.Password.RequiredUniqueChars = 1;
```

### Database Setup

#### 1. Create the Database

Ensure MySQL is running, then create the database:

```bash
mysql -u root -p
CREATE DATABASE MigratingAssistant;
EXIT;
```

#### 2. Install EF Core Tools (if not already installed)

Install the Entity Framework Core CLI tools globally:

```bash
dotnet tool install --global dotnet-ef
# Or update if already installed
dotnet tool update --global dotnet-ef
```

Verify installation:

```bash
dotnet ef --version
```

#### 3. Create and Apply Migrations

The project uses a custom migrations structure with MySQL-specific migrations. Follow these steps:

##### Option A: Use Existing Migrations (Recommended for first setup)

If migrations already exist in `src/Infrastructure/Data/Migrations/MySQL/`, simply apply them:

```bash
cd src/Web
dotnet ef database update --project ../Infrastructure
```

##### Option B: Create New Migrations (for schema changes)

When you modify entity models and need to create a new migration:

```bash
cd src/Web

# Create a new migration
dotnet ef migrations add YourMigrationName --project ../Infrastructure --output-dir Data/Migrations/MySQL

# Review the generated migration files in src/Infrastructure/Data/Migrations/MySQL/

# Apply the migration
dotnet ef database update --project ../Infrastructure
```

##### Common Migration Commands

```bash
# List all migrations and their status
dotnet ef migrations list --project ../Infrastructure

# Revert to a specific migration
dotnet ef database update PreviousMigrationName --project ../Infrastructure

# Remove the last unapplied migration
dotnet ef migrations remove --project ../Infrastructure

# Generate SQL script for migrations (useful for production)
dotnet ef migrations script --project ../Infrastructure --output migration.sql
```

#### 4. Verify Migration Success

After running migrations, verify the database schema:

```bash
mysql -u root -p MigratingAssistant
SHOW TABLES;
DESCRIBE Users;
EXIT;
```

You should see tables for: Users, UserProfiles, Roles, ServiceProviders, ServiceTypes, Listings, InventoryItems, Bookings, Jobs, JobApplications, Payments, Documents, SupportTickets, AuditLogs, and Identity tables.

#### 5. Seed Default Data

On first run, the application will automatically seed:

- **Roles**: Guest, User, Administrator
- **Default Admin Account**:
  - Email: `admin@migratingassistant.com`
  - Password: `Admin@123456`

**Important:** Change the default admin password immediately after first login!

#### Troubleshooting Migrations

**Migration fails with "Table already exists":**

```bash
# Reset the database (WARNING: destroys all data)
dotnet ef database drop --project ../Infrastructure
dotnet ef database update --project ../Infrastructure
```

**Migration doesn't detect model changes:**

```bash
# Clean and rebuild
dotnet clean
dotnet build
# Try creating migration again
```

**Connection string errors:**

- Verify MySQL is running: `systemctl status mysql` (Linux) or check Services (Windows)
- Test connection: `mysql -u root -p -h localhost`
- Check `appsettings.Development.json` has correct credentials

### Running the Application

#### Development Mode

```bash
cd src/Web
dotnet watch run
```

Navigate to:

- **API**: https://localhost:5001
- **Swagger UI**: https://localhost:5001/swagger

#### Production Build

```bash
dotnet build -c Release
cd src/Web
dotnet run -c Release
```

## API Endpoints

The API is organized into the following endpoint groups:

### Authentication (`/api/authentication`)

- `POST /login` - Authenticate user and receive JWT tokens
- `POST /register` - Register as a User
- `POST /register-guest` - Register as a Guest
- `POST /refresh` - Refresh access token using refresh token
- `POST /revoke` - Revoke a refresh token

### Core Entities

All entity endpoints follow RESTful conventions with full CRUD operations (except where noted):

| Entity               | Base Path               | ID Type | Operations                              |
| -------------------- | ----------------------- | ------- | --------------------------------------- |
| **Users**            | `/api/users`            | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **UserProfiles**     | `/api/userprofiles`     | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **ServiceTypes**     | `/api/servicetypes`     | int     | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **ServiceProviders** | `/api/serviceproviders` | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **Listings**         | `/api/listings`         | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **Jobs**             | `/api/jobs`             | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **JobApplications**  | `/api/jobapplications`  | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **Bookings**         | `/api/bookings`         | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **InventoryItems**   | `/api/inventoryitems`   | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **Documents**        | `/api/documents`        | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **Payments**         | `/api/payments`         | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **SupportTickets**   | `/api/supporttickets`   | Guid    | GET, GET/:id, POST, PUT/:id, DELETE/:id |
| **AuditLogs**        | `/api/auditlogs`        | long    | GET, GET/:id (Read-Only)                |

### Example API Requests

#### Login

```http
POST /api/authentication/login
Content-Type: application/json

{
  "email": "admin@migratingassistant.com",
  "password": "Admin@123456"
}
```

Response:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-token",
  "expiresIn": 3600
}
```

#### Create a Service Type

```http
POST /api/servicetypes
Authorization: Bearer {your-access-token}
Content-Type: application/json

{
  "serviceKey": "visa-consultation",
  "displayName": "Visa Consultation Service",
  "schemaHint": "{\"duration\": \"60min\"}",
  "enabled": true
}
```

#### Get All Listings

```http
GET /api/listings
Authorization: Bearer {your-access-token}
```

## Authentication

The platform uses **JWT Bearer Token Authentication** with refresh token rotation for security.

### Authentication Flow

1. **Register**: Create an account via `/api/authentication/register` or `/api/authentication/register-guest`
2. **Login**: Obtain access token and refresh token via `/api/authentication/login`
3. **Access Protected Resources**: Include `Authorization: Bearer {access-token}` header in requests
4. **Refresh Token**: When access token expires, use refresh token to get a new pair via `/api/authentication/refresh`
5. **Logout**: Revoke refresh token via `/api/authentication/revoke`

### Token Configuration

- **Access Token Lifetime**: 60 minutes
- **Refresh Token Lifetime**: 7 days
- **Token Rotation**: Enabled (old refresh tokens are revoked when refreshed)

### Authorization Roles

The platform implements three-tier role-based authorization:

| Role              | Description                          | Access Level                            |
| ----------------- | ------------------------------------ | --------------------------------------- |
| **Guest**         | Public users who register themselves | Basic read access                       |
| **User**          | Standard authenticated users         | Full access to personal data            |
| **Administrator** | System administrators                | Full system access including audit logs |

For detailed role-based authorization implementation, see [ROLE_BASED_AUTHORIZATION.md](ROLE_BASED_AUTHORIZATION.md).

## Exposed Entities

### Core Entities Overview

#### 1. **Users**

User accounts with ASP.NET Identity integration.

- Email-based authentication
- Password requirements enforced
- Role assignments

#### 2. **UserProfiles**

Extended user information for personalization.

- First name, last name
- Phone, address, city, country
- Date of birth

#### 3. **ServiceTypes** (int ID)

Configuration for different service categories.

- Service key (unique identifier)
- Display name
- Schema hint (JSON configuration)
- Enabled/disabled status

#### 4. **ServiceProviders**

Organizations or individuals offering immigration services.

- Company name, contact info
- Service type relationships
- Status tracking

#### 5. **Listings**

Individual service offerings in the marketplace.

- Linked to ServiceType and ServiceProvider
- Pricing information (Price, Currency)
- Availability windows
- Status (Draft, Active, Inactive)
- Custom attributes (JSON)

#### 6. **Jobs**

Immigration-related job postings.

- Linked to Listings
- Job type, responsibilities, requirements
- Posted date tracking

#### 7. **JobApplications**

User applications for job postings.

- Applicant tracking
- Application status
- Resume/cover letter references

#### 8. **Bookings**

Service appointments and reservations.

- Start/end date times
- Booking status (Pending, Confirmed, Cancelled, Completed)
- Linked to Listings and InventoryItems

#### 9. **InventoryItems**

Available resources for bookings.

- SKU tracking
- Metadata (JSON)
- Active/inactive status
- Linked to Listings

#### 10. **Documents**

Immigration document management.

- Document type, title
- File path and MIME type
- Upload tracking
- Expiration dates

#### 11. **Payments**

Payment transaction records.

- Amount and currency
- Payment method and status
- Transaction reference
- Payment date

#### 12. **SupportTickets**

Customer support system.

- Subject, description
- Priority levels
- Status tracking (Open, InProgress, Resolved, Closed)
- Assigned agent tracking

#### 13. **AuditLogs** (Read-Only)

System audit trail for compliance.

- Entity and action tracking
- Payload data (JSON)
- Timestamp logging
- Used for security and compliance auditing

### Entity Relationships

```
ServiceType (1) ─→ (*) Listings
ServiceProvider (1) ─→ (*) Listings
Listing (1) ─→ (*) InventoryItems
Listing (1) ─→ (*) Bookings
Listing (1) ─→ (*) Jobs
Job (1) ─→ (*) JobApplications
User (1) ─→ (1) UserProfile
User (1) ─→ (*) Bookings
User (1) ─→ (*) Documents
User (1) ─→ (*) Payments
User (1) ─→ (*) SupportTickets
```

## Development

### Project Structure

The solution follows Clean Architecture principles:

```
├── src/
│   ├── Domain/              # Enterprise business rules
│   │   ├── Entities/        # Domain entities
│   │   ├── Enums/           # Domain enumerations
│   │   ├── Events/          # Domain events
│   │   └── Exceptions/      # Domain exceptions
│   ├── Application/         # Application business rules
│   │   ├── Common/          # Shared interfaces and behaviors
│   │   ├── {Entity}/        # Feature folders (Commands, Queries)
│   │   └── DependencyInjection.cs
│   ├── Infrastructure/      # External concerns
│   │   ├── Data/            # EF Core DbContext and migrations
│   │   ├── Identity/        # ASP.NET Core Identity
│   │   └── Authentication/  # JWT implementation
│   └── Web/                 # Presentation layer
│       ├── Endpoints/       # Minimal API endpoints
│       ├── Infrastructure/  # Web-specific services
│       └── Program.cs       # Application entry point
└── tests/
    ├── Application.UnitTests/
    ├── Domain.UnitTests/
    └── Infrastructure.IntegrationTests/
```

### Adding a New Entity

1. **Create Domain Entity** in `src/Domain/Entities/`
2. **Add DbSet** to `ApplicationDbContext.cs`
3. **Create Migration**: `dotnet ef migrations add AddYourEntity`
4. **Create Commands** in `src/Application/{Entity}/Commands/`
5. **Create Queries** in `src/Application/{Entity}/Queries/`
6. **Create DTO** with AutoMapper profile
7. **Create Endpoints** in `src/Web/Endpoints/{Entity}.cs`
8. **Add Unit Tests** in `tests/Application.UnitTests/{Entity}/`

### Code Scaffolding

The template includes support to scaffold new commands and queries:

```bash
# Create a new command
dotnet new ca-usecase --name CreateYourEntity --feature-name YourEntity --usecase-type command --return-type Guid

# Create a new query
dotnet new ca-usecase -n GetYourEntities -fn YourEntity -ut query -rt YourEntityDto
```

If templates are not installed:

```bash
dotnet new install Clean.Architecture.Solution.Template::9.0.12
```

### Code Styles & Formatting

The solution includes [EditorConfig](https://editorconfig.org/) support. The `.editorconfig` file defines coding styles for the project.

## Testing

The solution contains comprehensive tests:

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
# Unit tests
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj

# Integration tests
dotnet test tests/Infrastructure.IntegrationTests/Infrastructure.IntegrationTests.csproj
```

### Test Coverage

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# Install report generator (once)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:./TestResults/CoverageReport -reporttypes:Html
```

### Current Test Coverage

- **Application Layer**: 42.9% (109 passing tests)
- **Command Handlers**: 85-100% coverage
- **Query Handlers**: Require integration tests with real database

## Troubleshooting

### Database Connection Issues

**Error**: "Unable to connect to MySQL server"

- Verify MySQL is running: `systemctl status mysql` (Linux) or check Windows Services
- Verify connection string credentials
- Check firewall settings

### JWT Token Issues

**Error**: "401 Unauthorized"

- Verify token is included in `Authorization: Bearer {token}` header
- Check token expiration (60 minutes default)
- Use refresh token endpoint to get new access token

**Error**: "IDX10500: Signature validation failed"

- Ensure JWT Secret matches between token generation and validation
- Verify Secret is at least 32 characters long

### Migration Issues

**Error**: "A migration has already been applied to the database"

- Drop database and reapply: `dotnet ef database drop` then `dotnet ef database update`
- Or create a new migration: `dotnet ef migrations add YourMigrationName`

## Production Deployment

### Environment Variables

Set the following environment variables for production:

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="your-production-connection-string"
JwtSettings__Secret="your-production-secret-min-32-chars"
```

### Security Checklist

- ✅ Change default admin password
- ✅ Use strong JWT secret (32+ characters, randomly generated)
- ✅ Enable HTTPS only
- ✅ Configure CORS appropriately
- ✅ Set secure password requirements
- ✅ Enable rate limiting
- ✅ Configure logging and monitoring
- ✅ Review and apply role-based authorization to all endpoints

## Additional Resources

- [Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [Role-Based Authorization Guide](ROLE_BASED_AUTHORIZATION.md)

## License

This project was generated using the Clean Architecture Solution Template version 9.0.12.

## Support

For issues, questions, or contributions, please refer to the project repository or contact the development team.
