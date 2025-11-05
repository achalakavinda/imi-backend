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
- [Docker Deployment](#docker-deployment) 🐳
  - [Quick Start with Docker](#quick-start-with-docker)
  - [Docker Development Setup](#docker-development-setup)
  - [Docker Production Deployment](#docker-production-deployment)
  - [Docker Commands Reference](#docker-commands-reference)
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

## Docker Deployment 🐳

The application is fully containerized with Docker support for both development and production environments.

### Prerequisites for Docker

Before getting started with Docker, ensure you have:

- **Windows**: [Docker Desktop for Windows](https://docs.docker.com/desktop/install/windows-install/)
  - Requires Windows 10/11 64-bit Pro, Enterprise, or Education
  - Enable WSL 2 (Windows Subsystem for Linux)
  - Enable Hyper-V and Containers Windows features
- **Mac**: [Docker Desktop for Mac](https://docs.docker.com/desktop/install/mac-install/)
  - Requires macOS 11 or newer
- **Linux**: [Docker Engine](https://docs.docker.com/engine/install/) + [Docker Compose](https://docs.docker.com/compose/install/)
  ```bash
  # Ubuntu/Debian example
  sudo apt-get update
  sudo apt-get install docker-ce docker-ce-cli containerd.io docker-compose-plugin
  ```

**Verify Installation:**

```bash
docker --version
docker compose version
```

### Quick Start with Docker (Recommended for Beginners)

This is the fastest way to get the application running locally using Docker.

#### Step 1: Verify Docker is Running

```bash
# Windows/Mac: Ensure Docker Desktop is running (check system tray)

# Test Docker installation
docker run hello-world
```

#### Step 2: Navigate to Project Directory

```bash
cd path/to/MigratingAssistant
```

#### Step 3: Build and Start All Services

```bash
# Build images and start all containers in detached mode
docker-compose up -d --build
```

**What this does:**

- ✅ Builds the .NET 8 application Docker image
- ✅ Pulls MySQL 8.0 image from Docker Hub
- ✅ Pulls phpMyAdmin image for database management
- ✅ Creates Docker network for service communication
- ✅ Creates persistent volume for MySQL data
- ✅ Starts all services in the background
- ✅ Runs database migrations automatically

**Expected output:**

```
[+] Building 45.3s (17/17) FINISHED
[+] Running 4/4
 ✔ Network migratingassistant_app-network    Created
 ✔ Container migratingassistant-mysql        Started
 ✔ Container migratingassistant-api          Started
 ✔ Container migratingassistant-phpmyadmin   Started
```

#### Step 4: Monitor Startup (First Time)

The first startup takes a few minutes for migrations to complete:

```bash
# Watch API logs in real-time
docker-compose logs -f api
```

**Look for these success messages:**

```
✅ MySQL connection successful
✅ Running database migrations...
✅ Migration completed successfully
✅ Seeding default data...
✅ Application started. Press Ctrl+C to shut down.
✅ Now listening on: http://[::]:8080
```

Press `Ctrl+C` to exit log viewing (containers keep running).

#### Step 5: Access the Application

Once the startup logs show "Application started", you can access:

| Service        | URL                           | Description                   |
| -------------- | ----------------------------- | ----------------------------- |
| **API**        | http://localhost:5000         | REST API endpoint             |
| **Swagger UI** | http://localhost:5000/swagger | Interactive API documentation |
| **phpMyAdmin** | http://localhost:8080         | Database management interface |

**Default Credentials:**

- **Admin User** (via API/Swagger):

  - Email: `admin@migratingassistant.com`
  - Password: `Admin@123456`

- **phpMyAdmin**:
  - Server: `mysql`
  - Username: `root`
  - Password: `rootpassword`

#### Step 6: Test the API

**Option A: Using Swagger UI** (Easiest)

1. Open http://localhost:5000/swagger
2. Find the `POST /api/users/login` endpoint
3. Click "Try it out"
4. Enter credentials:
   ```json
   {
     "email": "admin@migratingassistant.com",
     "password": "Admin@123456"
   }
   ```
5. Click "Execute"
6. Copy the `token` from the response
7. Click "Authorize" button at the top
8. Enter: `Bearer <your-token>`
9. Now you can test all protected endpoints!

**Option B: Using curl**

```bash
# Login and get token
curl -X POST http://localhost:5000/api/users/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@migratingassistant.com","password":"Admin@123456"}'

# Test protected endpoint (replace <token> with actual token)
curl -X GET http://localhost:5000/api/servicetypes \
  -H "Authorization: Bearer <token>"
```

**Option C: Using PowerShell (Windows)**

```powershell
# Login and get token
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/users/login" -Method POST -Body (@{email="admin@migratingassistant.com"; password="Admin@123456"} | ConvertTo-Json) -ContentType "application/json"
$token = $response.token

# Test protected endpoint
Invoke-RestMethod -Uri "http://localhost:5000/api/servicetypes" -Headers @{Authorization="Bearer $token"}
```

#### Step 7: Stop Services

```bash
# Stop all containers (data is preserved)
docker-compose down

# Stop and DELETE all data (start fresh next time)
docker-compose down -v
```

### Common First-Time Issues

**Issue: Port already in use**

```bash
# Error: bind: address already in use
# Solution: Change ports in docker-compose.yml or stop conflicting service

# Check what's using port 5000 (Windows)
netstat -ano | findstr :5000

# Check what's using port 5000 (Mac/Linux)
lsof -i :5000

# Or change the port
# Edit docker-compose.yml and change API_PORT to 5001
```

**Issue: Docker daemon not running**

```
ERROR: Cannot connect to the Docker daemon
```

**Solution**: Start Docker Desktop application (Windows/Mac) or start Docker service (Linux):

```bash
# Linux
sudo systemctl start docker
sudo systemctl enable docker
```

**Issue: Migrations fail**

```bash
# View detailed migration logs
docker-compose logs api

# Manually run migrations
docker-compose exec api dotnet ef database update --project /app/Infrastructure
```

**Issue: Can't access Swagger UI**

```bash
# Verify API is running and healthy
docker ps

# Check if API is listening
curl http://localhost:5000/health

# View API logs for errors
docker-compose logs api
```

### Docker Development Setup (Local Development)

The development environment includes everything you need to develop locally with Docker.

#### What's Included

- ✅ **Automatic database migrations** on container startup
- ✅ **MySQL 8.0 database** with persistent storage (data survives container restarts)
- ✅ **phpMyAdmin** for visual database management
- ✅ **Health checks** ensuring services are ready before startup
- ✅ **Isolated networking** (containers communicate via Docker network)
- ✅ **Environment variable configuration** via `.env` file
- ✅ **Volume mounts** for development files (optional hot reload)

#### Complete Local Development Workflow

**Daily Development Tasks:**

```bash
# 1. Start your development environment (first time or after reboot)
docker-compose up -d

# 2. Verify everything is running
docker-compose ps

# Expected output:
# NAME                              STATUS              PORTS
# migratingassistant-api            Up (healthy)        0.0.0.0:5000->8080/tcp
# migratingassistant-mysql          Up (healthy)        0.0.0.0:3306->3306/tcp
# migratingassistant-phpmyadmin     Up                  0.0.0.0:8080->80/tcp

# 3. Watch logs while developing (optional)
docker-compose logs -f api

# 4. Make code changes in your IDE (Visual Studio, VS Code, etc.)
# The changes are reflected in the container via volume mounts

# 5. To see changes, rebuild and restart the API container
docker-compose up -d --build api

# 6. Stop services at end of day (data is preserved)
docker-compose down
```

**Working with the Database:**

```bash
# Access MySQL CLI directly
docker-compose exec mysql mysql -u root -p
# Password: rootpassword (or from your .env)

# Inside MySQL CLI:
USE MigratingAssistant;
SHOW TABLES;
SELECT * FROM Users;
EXIT;

# Or use phpMyAdmin GUI
# Open: http://localhost:8080
# Login with root/rootpassword
```

**Running Commands in Containers:**

```bash
# Open bash shell inside API container
docker-compose exec api bash

# Inside container, you can run:
dotnet --version
dotnet ef migrations list --project Infrastructure
ls -la
exit

# Run single command without interactive shell
docker-compose exec api dotnet ef database update --project Infrastructure

# Run tests inside container
docker-compose exec api dotnet test
```

**Managing Database Migrations:**

```bash
# Create a new migration (from your host machine)
# Navigate to src/Web first
cd src/Web
dotnet ef migrations add YourMigrationName --project ../Infrastructure --output-dir Data/Migrations/MySQL

# Apply migrations (inside container)
docker-compose exec api dotnet ef database update --project Infrastructure

# Or restart container to auto-apply migrations
docker-compose restart api

# View migration status
docker-compose exec api dotnet ef migrations list --project Infrastructure
```

**Rebuilding After Changes:**

```bash
# Code changes only (C# files) - rebuild API
docker-compose up -d --build api

# Dockerfile changes - rebuild from scratch
docker-compose build --no-cache api
docker-compose up -d

# Database schema changes - just restart (migrations auto-run)
docker-compose restart api

# Environment variable changes - recreate containers
docker-compose down
docker-compose up -d
```

**Troubleshooting During Development:**

```bash
# Check container status and health
docker-compose ps

# View logs for specific service
docker-compose logs api          # Last logs
docker-compose logs -f api       # Follow logs
docker-compose logs --tail=50 api  # Last 50 lines

# View all service logs
docker-compose logs -f

# Restart a problematic service
docker-compose restart api

# Stop and remove everything, then start fresh
docker-compose down
docker-compose up -d --build

# Nuclear option: Delete everything including database
docker-compose down -v
docker-compose up -d --build
```

#### Configuring Environment Variables

Docker Compose uses environment variables for configuration. You can customize these values:

**Option 1: Use Default Values (No .env file needed)**

The `docker-compose.yml` has sensible defaults that work out of the box. Just run `docker-compose up -d`.

**Option 2: Create a `.env` File (Recommended)**

Create a `.env` file in the project root to customize settings:

```bash
# Copy the example file
cp .env.example .env

# Edit with your preferred editor
nano .env
# or
notepad .env
```

**Available Environment Variables:**

```bash
# ============================================
# MySQL Database Configuration
# ============================================
MYSQL_ROOT_PASSWORD=rootpassword
MYSQL_DATABASE=MigratingAssistant
MYSQL_USER=appuser
MYSQL_PASSWORD=apppassword
MYSQL_PORT=3306

# ============================================
# Application Configuration
# ============================================
API_PORT=5000
ASPNETCORE_ENVIRONMENT=Development

# ============================================
# JWT Authentication (32+ characters required)
# ============================================
JWT_SECRET=YourSuperSecretKeyThatIsAtLeast32CharactersLongForDevelopment
JWT_ISSUER=MigratingAssistant
JWT_AUDIENCE=MigratingAssistantUsers
JWT_EXPIRY_MINUTES=60
JWT_REFRESH_EXPIRY_DAYS=7

# ============================================
# phpMyAdmin (Database UI)
# ============================================
PHPMYADMIN_PORT=8080
```

**After changing `.env`, restart containers:**

```bash
docker-compose down
docker-compose up -d
```

**Common Customizations:**

```bash
# Change API port (if 5000 is in use)
API_PORT=5001

# Change MySQL port (if 3306 is in use)
MYSQL_PORT=3307

# Use stronger passwords
MYSQL_ROOT_PASSWORD=MyStr0ng!RootP@ssw0rd
MYSQL_PASSWORD=MyStr0ng!AppP@ssw0rd

# Change database name
MYSQL_DATABASE=MyCustomDatabaseName
```

#### Docker Compose Services

| Service        | Description            | Port | Container Name                |
| -------------- | ---------------------- | ---- | ----------------------------- |
| **api**        | .NET 8 Application     | 5000 | migratingassistant-api        |
| **mysql**      | MySQL 8.0 Database     | 3306 | migratingassistant-mysql      |
| **phpmyadmin** | Database Management UI | 8080 | migratingassistant-phpmyadmin |

### Docker Production Deployment

For production, use the production-specific compose file:

```bash
# Copy environment template
cp .env.example .env

# Edit .env with SECURE production values
nano .env

# Start with production configuration
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f api

# Stop services
docker-compose -f docker-compose.prod.yml down
```

#### Production Features

- ✅ **Resource limits** (CPU and memory constraints)
- ✅ **Automatic restart** on failure
- ✅ **Log rotation** (max 10MB per file, 3 files retained)
- ✅ **Health checks** with restart policies
- ✅ **Non-root user** for security
- ✅ **Nginx reverse proxy** (optional, configured in `docker-compose.prod.yml`)
- ✅ **SSL/TLS support** (configure in `nginx.conf`)
- ✅ **Network isolation** (MySQL not exposed to host)

#### Production Security Checklist

Before deploying to production:

- [ ] Change all default passwords in `.env`
- [ ] Use strong JWT secret (64+ characters recommended)
- [ ] Configure SSL certificates for HTTPS
- [ ] Review and adjust resource limits in `docker-compose.prod.yml`
- [ ] Disable phpMyAdmin (remove from compose file)
- [ ] Configure firewall rules for exposed ports
- [ ] Set up database backups
- [ ] Configure application logging to external service
- [ ] Review Nginx configuration in `nginx.conf`
- [ ] Use Docker secrets for sensitive data (advanced)

#### Production Environment Variables

```bash
# Required: Set these in production .env
MYSQL_ROOT_PASSWORD=<strong-random-password>
MYSQL_PASSWORD=<strong-random-password>
JWT_SECRET=<64-character-random-string>

# Optional: Custom domain and SSL
ALLOWED_HOSTS=yourdomain.com,www.yourdomain.com
SSL_CERT_PATH=/path/to/ssl/cert.pem
SSL_KEY_PATH=/path/to/ssl/key.pem
```

### Docker Commands Reference

#### Container Management

```bash
# List running containers
docker ps

# List all containers (including stopped)
docker ps -a

# Start specific service
docker-compose start api

# Stop specific service
docker-compose stop api

# Restart service
docker-compose restart api

# Remove stopped containers
docker-compose rm
```

#### Logs and Debugging

```bash
# View logs for all services
docker-compose logs

# Follow logs in real-time
docker-compose logs -f

# View logs for specific service
docker-compose logs -f api

# View last 100 lines
docker-compose logs --tail=100 api

# Execute shell in running container
docker-compose exec api bash

# Execute command without interactive shell
docker-compose exec api dotnet --version
```

#### Database Management

```bash
# Access MySQL CLI
docker-compose exec mysql mysql -u root -p

# Backup database
docker-compose exec mysql mysqldump -u root -p MigratingAssistant > backup.sql

# Restore database
docker-compose exec -T mysql mysql -u root -p MigratingAssistant < backup.sql

# Run migrations manually
docker-compose exec api dotnet ef database update --project /app/Infrastructure
```

#### Image and Volume Management

```bash
# Build/rebuild images
docker-compose build

# Build without cache (force rebuild)
docker-compose build --no-cache

# Pull latest images
docker-compose pull

# List volumes
docker volume ls

# Inspect volume
docker volume inspect migratingassistant_mysql-data

# Remove all unused volumes (CAUTION!)
docker volume prune
```

#### Cleanup

```bash
# Stop and remove containers, networks
docker-compose down

# Stop and remove containers, networks, volumes (DELETES DATA!)
docker-compose down -v

# Remove all stopped containers
docker container prune

# Remove unused images
docker image prune

# Remove everything unused (CAUTION!)
docker system prune -a
```

#### Health Checks

```bash
# Check container health status
docker ps --format "table {{.Names}}\t{{.Status}}"

# Inspect health check details
docker inspect --format='{{json .State.Health}}' migratingassistant-api

# View health check logs
docker inspect migratingassistant-api | grep -A 20 Health
```

#### Troubleshooting Docker Issues

**Container won't start:**

```bash
# Check logs for errors
docker-compose logs api

# Check container status
docker ps -a

# Inspect container
docker inspect migratingassistant-api
```

**Database connection errors:**

```bash
# Verify MySQL is healthy
docker ps --filter "name=mysql"

# Check MySQL logs
docker-compose logs mysql

# Test MySQL connection
docker-compose exec mysql mysql -u root -p -e "SHOW DATABASES;"
```

**Port already in use:**

```bash
# Check what's using the port (Windows)
netstat -ano | findstr :5000

# Check what's using the port (Linux/Mac)
lsof -i :5000

# Change port in .env file
echo "API_PORT=5001" >> .env
docker-compose up -d
```

**Out of disk space:**

```bash
# Check Docker disk usage
docker system df

# Clean up unused resources
docker system prune -a --volumes
```

**Migration failures:**

```bash
# Check API logs
docker-compose logs api

# Run migrations manually with verbose output
docker-compose exec api dotnet ef database update --verbose

# Drop and recreate database (CAUTION: Data loss!)
docker-compose exec mysql mysql -u root -p -e "DROP DATABASE MigratingAssistant; CREATE DATABASE MigratingAssistant;"
docker-compose restart api
```

### Docker Quick Reference Card

Essential commands for daily use:

| Task                         | Command                                                                                              | Description                          |
| ---------------------------- | ---------------------------------------------------------------------------------------------------- | ------------------------------------ |
| **Start all services**       | `docker-compose up -d`                                                                               | Start in background (detached mode)  |
| **Start with rebuild**       | `docker-compose up -d --build`                                                                       | Rebuild images before starting       |
| **Stop all services**        | `docker-compose down`                                                                                | Stop and remove containers           |
| **Stop + delete data**       | `docker-compose down -v`                                                                             | Stop and remove volumes (data loss!) |
| **View all logs**            | `docker-compose logs -f`                                                                             | Follow logs in real-time             |
| **View API logs**            | `docker-compose logs -f api`                                                                         | Follow API logs only                 |
| **Check status**             | `docker-compose ps`                                                                                  | List running containers              |
| **Restart service**          | `docker-compose restart api`                                                                         | Restart specific service             |
| **Rebuild single service**   | `docker-compose up -d --build api`                                                                   | Rebuild only API container           |
| **Open API shell**           | `docker-compose exec api bash`                                                                       | Interactive shell in API container   |
| **Open MySQL CLI**           | `docker-compose exec mysql mysql -u root -p`                                                         | Access MySQL command line            |
| **Run command in container** | `docker-compose exec api <command>`                                                                  | Execute command without shell        |
| **View container IP**        | `docker inspect -f '{{range.NetworkSettings.Networks}}{{.IPAddress}}{{end}}' migratingassistant-api` | Get container IP address             |
| **Check health**             | `docker ps --format "table {{.Names}}\t{{.Status}}"`                                                 | View health status of all containers |
| **Backup database**          | `docker-compose exec mysql mysqldump -u root -p MigratingAssistant > backup.sql`                     | Export database to file              |
| **Restore database**         | `docker-compose exec -T mysql mysql -u root -p MigratingAssistant < backup.sql`                      | Import database from file            |
| **Clean up everything**      | `docker system prune -a --volumes`                                                                   | Remove all unused Docker resources   |

**Production commands:**

```bash
# Start production environment
docker-compose -f docker-compose.prod.yml up -d

# View production logs
docker-compose -f docker-compose.prod.yml logs -f

# Stop production environment
docker-compose -f docker-compose.prod.yml down
```

**URLs to remember:**

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- phpMyAdmin: http://localhost:8080
- Health Check: http://localhost:5000/health

## Testing the API

### Quick Verification Guide

Follow these steps to verify your installation is working correctly.

#### Step 1: Check Health Endpoint

**PowerShell:**

```powershell
Invoke-WebRequest -Uri http://localhost:5000/health -UseBasicParsing
```

**Bash/Linux/Mac:**

```bash
curl http://localhost:5000/health
```

**Expected Response:** `200 OK` with body `Healthy`

#### Step 2: Get Authentication Token

The system comes with a pre-seeded administrator account:

- **Email:** `admin@migratingassistant.com`
- **Password:** `Admin@123456`
- **Role:** Administrator (full access)

**PowerShell:**

```powershell
# Login and save response
$response = Invoke-RestMethod -Method POST -Uri http://localhost:5000/api/authentication/login `
  -Headers @{"Content-Type"="application/json"} `
  -Body '{"email":"admin@migratingassistant.com","password":"Admin@123456"}'

# Extract and display token
$token = $response.accessToken
Write-Host "`nAccess Token obtained successfully!`n"
Write-Host "Token: $token`n"

# Save token for later use
$env:API_TOKEN = $token
```

**Bash/Linux/Mac:**

```bash
# Login and save response
RESPONSE=$(curl -s -X POST http://localhost:5000/api/authentication/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@migratingassistant.com","password":"Admin@123456"}')

# Extract access token (requires jq: sudo apt-get install jq)
TOKEN=$(echo $RESPONSE | jq -r '.accessToken')

# Display token
echo "Access Token obtained successfully!"
echo "Token: $TOKEN"

# Save token for later use
export API_TOKEN=$TOKEN
```

**Expected Response:**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "8Xq2p9vK5mN3jH7...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

#### Step 3: Access Protected Resources

Now use the token to access protected endpoints:

**PowerShell:**

```powershell
# List all users (Admin only)
Invoke-RestMethod -Method GET -Uri http://localhost:5000/api/users `
  -Headers @{"Authorization"="Bearer $env:API_TOKEN"} | ConvertTo-Json

# Get current user profile
Invoke-RestMethod -Method GET -Uri http://localhost:5000/api/users/me `
  -Headers @{"Authorization"="Bearer $env:API_TOKEN"} | ConvertTo-Json

# List service types
Invoke-RestMethod -Method GET -Uri http://localhost:5000/api/servicetypes `
  -Headers @{"Authorization"="Bearer $env:API_TOKEN"} | ConvertTo-Json

# List bookings
Invoke-RestMethod -Method GET -Uri http://localhost:5000/api/bookings `
  -Headers @{"Authorization"="Bearer $env:API_TOKEN"} | ConvertTo-Json
```

**Bash/Linux/Mac:**

```bash
# List all users (Admin only)
curl -X GET http://localhost:5000/api/users \
  -H "Authorization: Bearer $API_TOKEN" | jq

# Get current user profile
curl -X GET http://localhost:5000/api/users/me \
  -H "Authorization: Bearer $API_TOKEN" | jq

# List service types
curl -X GET http://localhost:5000/api/servicetypes \
  -H "Authorization: Bearer $API_TOKEN" | jq

# List bookings
curl -X GET http://localhost:5000/api/bookings \
  -H "Authorization: Bearer $API_TOKEN" | jq
```

**Expected Response:** `200 OK` with JSON array of resources

#### Step 4: Create a Resource (POST Request)

**PowerShell:**

```powershell
# Create a new service type
$newServiceType = @{
    serviceKey = "visa-consultation"
    displayName = "Visa Consultation"
    description = "Professional visa consultation services"
    enabled = $true
} | ConvertTo-Json

Invoke-RestMethod -Method POST -Uri http://localhost:5000/api/servicetypes `
  -Headers @{
      "Authorization"="Bearer $env:API_TOKEN"
      "Content-Type"="application/json"
  } `
  -Body $newServiceType | ConvertTo-Json
```

**Bash/Linux/Mac:**

```bash
# Create a new service type
curl -X POST http://localhost:5000/api/servicetypes \
  -H "Authorization: Bearer $API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "serviceKey": "visa-consultation",
    "displayName": "Visa Consultation",
    "description": "Professional visa consultation services",
    "enabled": true
  }' | jq
```

**Expected Response:** `201 Created` with the created resource

#### Step 5: Update a Resource (PUT Request)

**PowerShell:**

```powershell
# Update service type (replace {id} with actual ID)
$updateData = @{
    id = 1  # Replace with actual ID
    serviceKey = "visa-consultation"
    displayName = "Premium Visa Consultation"
    description = "Premium professional visa consultation services"
    enabled = $true
} | ConvertTo-Json

Invoke-RestMethod -Method PUT -Uri http://localhost:5000/api/servicetypes/1 `
  -Headers @{
      "Authorization"="Bearer $env:API_TOKEN"
      "Content-Type"="application/json"
  } `
  -Body $updateData
```

**Bash/Linux/Mac:**

```bash
# Update service type (replace {id} with actual ID)
curl -X PUT http://localhost:5000/api/servicetypes/1 \
  -H "Authorization: Bearer $API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "serviceKey": "visa-consultation",
    "displayName": "Premium Visa Consultation",
    "description": "Premium professional visa consultation services",
    "enabled": true
  }'
```

**Expected Response:** `204 No Content` (success)

#### Step 6: Delete a Resource (DELETE Request)

**PowerShell:**

```powershell
# Delete service type (replace {id} with actual ID)
Invoke-RestMethod -Method DELETE -Uri http://localhost:5000/api/servicetypes/1 `
  -Headers @{"Authorization"="Bearer $env:API_TOKEN"}
```

**Bash/Linux/Mac:**

```bash
# Delete service type (replace {id} with actual ID)
curl -X DELETE http://localhost:5000/api/servicetypes/1 \
  -H "Authorization: Bearer $API_TOKEN"
```

**Expected Response:** `204 No Content` (success)

### Complete Test Script

**PowerShell:**

```powershell
Write-Host "=== MigratingAssistant API Test Script ===" -ForegroundColor Cyan

# 1. Health Check
Write-Host "`n1. Testing health endpoint..." -ForegroundColor Yellow
$health = Invoke-WebRequest -Uri http://localhost:5000/health -UseBasicParsing
Write-Host "   Health Status: $($health.StatusCode) - $($health.Content)" -ForegroundColor Green

# 2. Login
Write-Host "`n2. Logging in as admin..." -ForegroundColor Yellow
$loginResponse = Invoke-RestMethod -Method POST -Uri http://localhost:5000/api/authentication/login `
  -Headers @{"Content-Type"="application/json"} `
  -Body '{"email":"admin@migratingassistant.com","password":"Admin@123456"}'

$token = $loginResponse.accessToken
Write-Host "   Login successful! Token expires in: $($loginResponse.expiresIn) seconds" -ForegroundColor Green

# 3. Get current user
Write-Host "`n3. Getting current user profile..." -ForegroundColor Yellow
$currentUser = Invoke-RestMethod -Method GET -Uri http://localhost:5000/api/users/me `
  -Headers @{"Authorization"="Bearer $token"}
Write-Host "   Logged in as: $($currentUser.email)" -ForegroundColor Green

# 4. List service types
Write-Host "`n4. Fetching service types..." -ForegroundColor Yellow
$serviceTypes = Invoke-RestMethod -Method GET -Uri http://localhost:5000/api/servicetypes `
  -Headers @{"Authorization"="Bearer $token"}
Write-Host "   Found $($serviceTypes.Count) service type(s)" -ForegroundColor Green

Write-Host "`n=== All tests passed! ===" -ForegroundColor Cyan
```

**Bash:**

```bash
#!/bin/bash
echo "=== MigratingAssistant API Test Script ==="

# 1. Health Check
echo -e "\n1. Testing health endpoint..."
HEALTH=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/health)
echo "   Health Status: $HEALTH"

# 2. Login
echo -e "\n2. Logging in as admin..."
RESPONSE=$(curl -s -X POST http://localhost:5000/api/authentication/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@migratingassistant.com","password":"Admin@123456"}')

TOKEN=$(echo $RESPONSE | jq -r '.accessToken')
EXPIRES=$(echo $RESPONSE | jq -r '.expiresIn')
echo "   Login successful! Token expires in: $EXPIRES seconds"

# 3. Get current user
echo -e "\n3. Getting current user profile..."
USER=$(curl -s -X GET http://localhost:5000/api/users/me \
  -H "Authorization: Bearer $TOKEN")
EMAIL=$(echo $USER | jq -r '.email')
echo "   Logged in as: $EMAIL"

# 4. List service types
echo -e "\n4. Fetching service types..."
SERVICES=$(curl -s -X GET http://localhost:5000/api/servicetypes \
  -H "Authorization: Bearer $TOKEN")
COUNT=$(echo $SERVICES | jq '. | length')
echo "   Found $COUNT service type(s)"

echo -e "\n=== All tests passed! ==="
```

### Using Swagger UI (Easiest Method)

1. Open http://localhost:5000/swagger in your browser
2. Click on **POST /api/authentication/login**
3. Click **"Try it out"**
4. Enter credentials:
   ```json
   {
     "email": "admin@migratingassistant.com",
     "password": "Admin@123456"
   }
   ```
5. Click **"Execute"**
6. Copy the `accessToken` from the response
7. Click the **"Authorize"** button (🔒 icon at top)
8. Enter: `Bearer YOUR_ACCESS_TOKEN_HERE`
9. Click **"Authorize"**
10. Now you can test all endpoints!

### Common HTTP Status Codes

| Code                          | Meaning                  | Common Causes                              |
| ----------------------------- | ------------------------ | ------------------------------------------ |
| **200 OK**                    | Success                  | GET request successful                     |
| **201 Created**               | Resource created         | POST request successful                    |
| **204 No Content**            | Success, no body         | PUT/DELETE successful                      |
| **400 Bad Request**           | Invalid input            | Validation failed, missing required fields |
| **401 Unauthorized**          | Authentication required  | Missing or invalid token                   |
| **403 Forbidden**             | Insufficient permissions | User role doesn't have access              |
| **404 Not Found**             | Resource doesn't exist   | Invalid ID or endpoint                     |
| **500 Internal Server Error** | Server error             | Check API logs                             |

### Troubleshooting API Issues

**Issue: 401 Unauthorized**

```powershell
# Check if token is expired (tokens expire after 60 minutes by default)
# Login again to get a new token
```

**Issue: 400 Bad Request - Validation Errors**

```powershell
# Check the response body for validation messages
# Ensure all required fields are provided
# Verify data types match (e.g., Guid format: "3fa85f64-5717-4562-b3fc-2c963f66afa6")
```

**Issue: Connection Refused**

```powershell
# Verify containers are running
docker-compose ps

# Check API logs
docker-compose logs api
```

## Understanding Docker Setup (Beginner's Guide)

This section explains how Docker containerization works in this project, even if you've never used Docker before.

### What is Docker?

**Docker** is a platform that packages applications and their dependencies into **containers**. Think of a container as a lightweight, portable box that includes:

- Your application code
- The .NET runtime
- Required libraries and dependencies
- Configuration files

**Why use Docker?**

- ✅ **Works everywhere**: "It works on my machine" → "It works in containers everywhere"
- ✅ **Isolated environment**: Your app runs in its own space, no conflicts with other software
- ✅ **Easy setup**: One command (`docker-compose up`) starts everything
- ✅ **Consistent**: Development, testing, and production use the same environment

**Docker vs Virtual Machines:**

- **VM**: Includes full OS, heavy (GBs), slow to start (minutes)
- **Container**: Shares host OS, lightweight (MBs), starts instantly (seconds)

**Key Docker Concepts:**

| Term               | Explanation                                             |
| ------------------ | ------------------------------------------------------- |
| **Image**          | Blueprint for a container (like a class in programming) |
| **Container**      | Running instance of an image (like an object)           |
| **Docker Compose** | Tool to define and run multi-container applications     |
| **Volume**         | Persistent storage that survives container restarts     |
| **Network**        | Allows containers to communicate with each other        |
| **Dockerfile**     | Recipe to build a Docker image                          |

### Project Docker Architecture

This project uses **3 containers** working together:

```
┌─────────────────────────────────────────────────────────┐
│  Your Computer (Host)                                   │
│                                                          │
│  ┌──────────────┐     ┌──────────────┐    ┌──────────┐│
│  │   API        │────→│   MySQL      │    │phpMyAdmin││
│  │ (ASP.NET)    │     │  (Database)  │←───│  (DB UI) ││
│  │ Port: 5000   │     │              │    │Port: 8080││
│  └──────────────┘     └──────────────┘    └──────────┘│
│         ↑                     ↑                         │
│         │                     │                         │
│         └─────────────────────┘                         │
│              app-network                                │
│           (Docker Network)                              │
└─────────────────────────────────────────────────────────┘
```

**Communication Flow:**

1. You access API at `http://localhost:5000`
2. API connects to MySQL using hostname `mysql` (Docker's internal DNS)
3. phpMyAdmin connects to MySQL at `http://localhost:8080`

### Dockerfile Explained (Line by Line)

The `Dockerfile` is a recipe that builds your API container in 3 stages.

#### Stage 1: Build (SDK Image)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
```

- **Purpose**: Use Microsoft's official .NET 8 SDK image (has compiler and tools)
- **Name**: This stage is named "build" for reference later

```dockerfile
WORKDIR /src
```

- **Purpose**: Set working directory to `/src` inside the container
- **Why**: All subsequent commands run from this directory

```dockerfile
COPY ["src/Web/Web.csproj", "src/Web/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
```

- **Purpose**: Copy only `.csproj` files first (not entire source code yet)
- **Why**: Docker caching! If project files haven't changed, this layer is cached and NuGet restore is skipped

```dockerfile
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]
```

- **Purpose**: Copy build configuration files
- **Why**: These files control NuGet versions and build settings

```dockerfile
RUN dotnet restore "src/Web/Web.csproj"
```

- **Purpose**: Download NuGet packages (dependencies)
- **Why**: Separate from build step for better caching
- **Note**: We restore only `Web.csproj` (not the solution) to avoid errors with test projects that may not be present

```dockerfile
COPY ["src/", "src/"]
```

- **Purpose**: Now copy all source code
- **Why**: Done after restore so changing code doesn't invalidate NuGet cache

```dockerfile
RUN dotnet build "src/Web/Web.csproj" -c Release -o /app/build
```

- **Purpose**: Compile the application
- **Options**:
  - `-c Release`: Build in Release mode (optimized, no debug symbols)
  - `-o /app/build`: Output compiled files to `/app/build`

#### Stage 2: Publish (Create Deployment Package)

```dockerfile
FROM build AS publish
```

- **Purpose**: Use the "build" stage as the starting point
- **Why**: Continue from compiled code, don't start over

```dockerfile
RUN dotnet publish "src/Web/Web.csproj" -c Release -o /app/publish /p:UseAppHost=false
```

- **Purpose**: Create deployment package (only runtime files, no build tools)
- **Options**:
  - `-c Release`: Publish Release build
  - `-o /app/publish`: Output to `/app/publish`
  - `/p:UseAppHost=false`: Don't create platform-specific executable (we use `dotnet` command)

#### Stage 3: Final Runtime Image

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
```

- **Purpose**: Use lightweight runtime image (no SDK, no compiler)
- **Why**: Final image is ~80% smaller! SDK is 700MB, runtime is 200MB

```dockerfile
RUN apt-get update && apt-get install -y curl netcat-openbsd && rm -rf /var/lib/apt/lists/*
```

- **Purpose**: Install tools needed for health checks
- **Tools**:
  - `curl`: HTTP requests for health checks
  - `netcat-openbsd`: Network connectivity testing
- **Cleanup**: `rm -rf /var/lib/apt/lists/*` removes package cache to keep image small

```dockerfile
RUN groupadd -r appuser && useradd -r -g appuser appuser
```

- **Purpose**: Create non-root user for security
- **Why**: Running as root is a security risk; if app is compromised, attacker has limited permissions

```dockerfile
WORKDIR /app
```

- **Purpose**: Set working directory to `/app`

```dockerfile
COPY --from=publish /app/publish .
```

- **Purpose**: Copy published files from "publish" stage
- **Key**: Only runtime files are copied, not SDK or source code
- **Result**: Small, secure final image

```dockerfile
COPY docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh
```

- **Purpose**: Copy and make entrypoint script executable
- **Why**: This script handles MySQL connection waiting

```dockerfile
RUN chown -R appuser:appuser /app
```

- **Purpose**: Give `appuser` ownership of `/app` directory
- **Why**: Non-root user needs permissions to read files

```dockerfile
USER appuser
```

- **Purpose**: Switch to non-root user
- **Result**: All subsequent commands (including app startup) run as `appuser`

```dockerfile
EXPOSE 8080
```

- **Purpose**: Document that container listens on port 8080
- **Note**: This is documentation only; actual port is controlled by ASP.NET environment variable

```dockerfile
ENTRYPOINT ["/app/docker-entrypoint.sh"]
```

- **Purpose**: Define what runs when container starts
- **Result**: Executes `docker-entrypoint.sh` (which waits for MySQL, then starts API)

```dockerfile
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
```

- **Purpose**: Docker periodically checks if container is healthy
- **Settings**:
  - `--interval=30s`: Check every 30 seconds
  - `--timeout=10s`: If check takes >10s, it's considered failed
  - `--start-period=40s`: Don't count failures for first 40s (app startup time)
  - `--retries=3`: After 3 consecutive failures, mark as unhealthy
- **Command**: Try to GET `/health` endpoint; if it fails, exit with code 1 (unhealthy)

**Multi-Stage Benefits:**

- **Fast builds**: Unchanged layers are cached
- **Small image**: Only runtime files in final image (SDK is discarded)
- **Secure**: No source code or build tools in production image

### docker-compose.yml Explained (Development)

Docker Compose lets you define multiple containers and how they connect.

#### File Structure

```yaml
version: "3.8"
```

- **Purpose**: Specifies Docker Compose file format version
- **Version 3.8**: Modern features, compatible with Docker Engine 19.03+

```yaml
services:
```

- **Purpose**: Defines the containers (services) that make up your application

#### MySQL Service

```yaml
mysql:
  image: mysql:8.0
```

- **Service name**: `mysql` (used as hostname in Docker network)
- **Image**: Official MySQL 8.0 from Docker Hub
- **Result**: Docker pulls this image if not already present

```yaml
container_name: migratingassistant-mysql
```

- **Purpose**: Custom name for the container (easier to identify)
- **Default**: Would be `migratingassistant_mysql_1` without this

```yaml
ports:
  - "${MYSQL_PORT:-3306}:3306"
```

- **Format**: `HOST_PORT:CONTAINER_PORT`
- **Meaning**: Map port 3306 inside container to port 3306 on your computer
- **Variable**: Uses `MYSQL_PORT` from `.env` file, defaults to 3306
- **Result**: You can connect to MySQL at `localhost:3306`

```yaml
environment:
  MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD:-rootpassword}
  MYSQL_DATABASE: ${MYSQL_DATABASE:-MigratingAssistant}
  MYSQL_USER: ${MYSQL_USER:-appuser}
  MYSQL_PASSWORD: ${MYSQL_PASSWORD:-apppassword}
```

- **Purpose**: Configure MySQL on first startup
- **Variables**: Read from `.env` file, with defaults shown after `:-`
- **Result**: Creates database, user, and sets passwords

```yaml
volumes:
  - mysql-data:/var/lib/mysql
```

- **Purpose**: Persist MySQL data across container restarts
- **Format**: `VOLUME_NAME:CONTAINER_PATH`
- **Result**: Data survives `docker-compose down` (but not `docker-compose down -v`)

```yaml
networks:
  - app-network
```

- **Purpose**: Connect this container to the `app-network` network
- **Result**: MySQL can communicate with API using service name

```yaml
healthcheck:
  test:
    [
      "CMD",
      "mysqladmin",
      "ping",
      "-h",
      "localhost",
      "-u",
      "root",
      "-p${MYSQL_ROOT_PASSWORD:-rootpassword}",
    ]
  interval: 10s
  timeout: 5s
  retries: 5
  start_period: 30s
```

- **Purpose**: Check if MySQL is ready to accept connections
- **Test**: Run `mysqladmin ping` command inside container
- **Settings**: Check every 10s, timeout after 5s, allow 30s startup time, 5 retries
- **Result**: API container waits for MySQL to be healthy before starting

#### API Service

```yaml
api:
  build:
    context: .
    dockerfile: Dockerfile
```

- **Purpose**: Build image from local `Dockerfile` instead of pulling from Docker Hub
- **Context**: `.` means current directory (project root)
- **Result**: Runs multi-stage build defined in Dockerfile

```yaml
container_name: migratingassistant-api
```

- **Purpose**: Custom container name

```yaml
ports:
  - "${API_PORT:-5000}:8080"
```

- **Format**: `HOST_PORT:CONTAINER_PORT`
- **Meaning**: Map port 8080 inside container to port 5000 on your computer
- **Why 8080?**: ASP.NET Core default port in containers
- **Result**: Access API at `http://localhost:5000`

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
```

- **Purpose**: Set environment to Development
- **Result**: Swagger UI is enabled, detailed error messages shown

```yaml
- Database__Provider=MySql
- Database__ConnectionString=Server=mysql;Port=3306;Database=${MYSQL_DATABASE:-MigratingAssistant};Uid=${MYSQL_USER:-appuser};Pwd=${MYSQL_PASSWORD:-apppassword};
```

- **Double underscore**: `Database__Provider` becomes `Database:Provider` in appsettings
- **Server=mysql**: Uses service name as hostname (Docker DNS resolves this)
- **Why not localhost?**: In containers, `localhost` means the container itself, not your computer

```yaml
- Jwt__Secret=${JWT_SECRET:-your-super-secret-key-at-least-32-characters-long-12345}
- Jwt__Issuer=${JWT_ISSUER:-MigratingAssistant}
- Jwt__Audience=${JWT_AUDIENCE:-MigratingAssistantUsers}
- Jwt__ExpiryMinutes=${JWT_EXPIRY_MINUTES:-60}
- Jwt__RefreshExpiryDays=${JWT_REFRESH_EXPIRY_DAYS:-7}
```

- **Purpose**: JWT authentication configuration
- **Secret**: Used to sign tokens (MUST be at least 32 characters)
- **Expiry**: Access token valid for 60 minutes, refresh token for 7 days

```yaml
depends_on:
  mysql:
    condition: service_healthy
```

- **Purpose**: Start API only after MySQL is healthy
- **Condition**: Waits for MySQL healthcheck to pass
- **Why**: Prevents connection errors during startup

```yaml
networks:
  - app-network
```

- **Purpose**: Connect to same network as MySQL
- **Result**: API can connect to MySQL using `Server=mysql`

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

- **Purpose**: Check if API is responding
- **Test**: HTTP GET to `/health` endpoint
- **Result**: Docker marks container as healthy/unhealthy

#### phpMyAdmin Service

```yaml
phpmyadmin:
  image: phpmyadmin:latest
  container_name: migratingassistant-phpmyadmin
  ports:
    - "${PHPMYADMIN_PORT:-8080}:80"
  environment:
    PMA_HOST: mysql
    PMA_PORT: 3306
    UPLOAD_LIMIT: 100M
  depends_on:
    - mysql
  networks:
    - app-network
```

- **Purpose**: Web-based MySQL administration tool
- **Access**: http://localhost:8080
- **PMA_HOST**: Connects to `mysql` service
- **Upload Limit**: Allow importing large SQL files

#### Networks

```yaml
networks:
  app-network:
    driver: bridge
```

- **Purpose**: Create isolated network for containers
- **Driver bridge**: Default network type (containers on same machine)
- **Result**: Containers can communicate using service names as hostnames

#### Volumes

```yaml
volumes:
  mysql-data:
```

- **Purpose**: Define named volume for MySQL data persistence
- **Location**: Docker manages storage location (usually `/var/lib/docker/volumes/`)
- **Lifecycle**: Data persists until explicitly deleted with `docker-compose down -v`

### docker-compose.prod.yml Explained (Production)

Production compose file builds on development with security and performance improvements.

#### Key Differences from Development

**MySQL Service - Not Exposed:**

```yaml
mysql:
  # NO ports: section
```

- **Security**: MySQL is NOT accessible from outside Docker network
- **Result**: Only API container can connect, external access blocked

**API Service - Resource Limits:**

```yaml
deploy:
  resources:
    limits:
      cpus: "2"
      memory: 1G
    reservations:
      cpus: "0.5"
      memory: 512M
  restart_policy:
    condition: on-failure
```

- **Limits**: Container cannot use more than 2 CPUs or 1GB RAM
- **Reservations**: Docker guarantees at least 0.5 CPU and 512MB RAM
- **Why**: Prevents single container from consuming all server resources

**API Service - SSL Required:**

```yaml
- Database__ConnectionString=Server=mysql;Port=3306;Database=${MYSQL_DATABASE};Uid=${MYSQL_USER};Pwd=${MYSQL_PASSWORD};SslMode=Required
```

- **SslMode=Required**: Encrypts MySQL connection
- **Why**: Protect credentials and data in transit

**API Service - Log Rotation:**

```yaml
logging:
  driver: "json-file"
  options:
    max-size: "10m"
    max-file: "3"
```

- **max-size**: Rotate logs when file reaches 10MB
- **max-file**: Keep maximum 3 log files (total 30MB)
- **Why**: Prevent logs from filling disk space

**No phpMyAdmin:**

- **Security**: Database admin tools removed from production
- **Alternative**: Use SSH tunnel for admin access when needed

### docker-entrypoint.sh Explained

This bash script runs when the API container starts.

```bash
#!/bin/bash
set -e
```

- **Shebang**: Indicates this is a bash script
- **set -e**: Exit immediately if any command fails (fail-fast behavior)

```bash
host="mysql"
port="3306"
max_attempts=30
attempt=0
```

- **Variables**: MySQL hostname, port, retry configuration
- **max_attempts**: Try for 60 seconds (30 × 2 seconds)

```bash
echo "Waiting for MySQL at $host:$port..."
while [ $attempt -lt $max_attempts ]; do
    if timeout 1 bash -c "</dev/tcp/$host/$port"; then
        echo "MySQL is available!"
        sleep 5  # Additional wait for full readiness
        break
    fi
    attempt=$((attempt + 1))
    echo "Attempt $attempt/$max_attempts: MySQL not ready, waiting..."
    sleep 2
done
```

- **Purpose**: Wait for MySQL to accept connections
- **Test**: Try to open TCP connection to `mysql:3306`
- **Logic**: Keep trying for 30 attempts, 2 seconds apart
- **Why**: MySQL container starts before it's ready to accept connections
- **Additional wait**: Extra 5 seconds after connection succeeds (MySQL initialization)

```bash
if [ $attempt -eq $max_attempts ]; then
    echo "ERROR: MySQL did not become available in time!"
    exit 1
fi
```

- **Failure handling**: If MySQL never responds, exit with error
- **Result**: Container stops and Docker Compose can retry

```bash
echo "Starting application..."
dll_file=$(find /app -maxdepth 1 -name "*.Web.dll" | head -n 1)
```

- **Purpose**: Find the main application DLL dynamically
- **Pattern**: Looks for `*.Web.dll` (e.g., `MigratingAssistant.Web.dll`)
- **Why dynamic**: Avoids hardcoding project name

```bash
if [ -z "$dll_file" ]; then
    echo "ERROR: Could not find Web.dll file!"
    exit 1
fi
```

- **Validation**: Ensure DLL was found
- **Failure**: Exit if no DLL (indicates build problem)

```bash
echo "Found DLL: $dll_file"
exec dotnet "$dll_file"
```

- **exec**: Replace current process with `dotnet` (forwards signals properly)
- **Start**: Run the application with `dotnet MigratingAssistant.Web.dll`

**Why No Migrations?**

- **dotnet ef not available**: Runtime image has no SDK tools
- **Alternative**: Application runs migrations automatically via `ApplicationDbContextInitialiser`
- **Location**: `src/Infrastructure/Data/ApplicationDbContextInitialiser.cs`
- **Timing**: Runs on application startup before handling requests

### Environment Variables (.env file)

Docker Compose reads `.env` file to set environment variables.

#### How It Works

**Priority Order:**

1. Environment variables set in shell
2. Variables in `.env` file
3. Default values in `docker-compose.yml` (after `:-`)

**Example:**

```yaml
ports:
  - "${API_PORT:-5000}:8080"
```

- If `API_PORT` is in `.env` → use that value
- If not in `.env` → use default `5000`

#### Creating Your .env File

```bash
# Copy example file
cp .env.example .env

# Edit with your values
notepad .env  # Windows
nano .env     # Linux/Mac
```

#### Double Underscore Notation

```bash
Database__Provider=MySql
Database__ConnectionString=Server=mysql;...
```

**Why double underscore?**

- **ASP.NET Convention**: `__` represents nested configuration
- **Mapping**: `Database__Provider` → `appsettings.json`:
  ```json
  {
    "Database": {
      "Provider": "MySql"
    }
  }
  ```

**Alternatives:**

- **appsettings.json**: `"Database": { "Provider": "MySql" }`
- **Environment Variable**: `Database__Provider=MySql`
- **Command Line**: `--Database:Provider=MySql`

### Container Networking Deep Dive

#### How Containers Communicate

**Same Network:**

```yaml
networks:
  app-network:
```

- All services on `app-network` can talk to each other
- Isolation from other Docker networks

**Service Names as Hostnames:**

```yaml
services:
  mysql: # ← Hostname is "mysql"
  api: # ← Hostname is "api"
  phpmyadmin: # ← Hostname is "phpmyadmin"
```

**Docker's Built-in DNS:**

- Docker runs DNS server inside network
- Resolves service names to container IPs
- Updates automatically when containers restart

**Example Connection:**

```csharp
// In API container
Server=mysql  // ← Docker resolves "mysql" to MySQL container's IP
```

**Why Not Localhost?**

- `localhost` in a container means the container itself
- To reach another container, use service name or IP

#### Port Mapping

**Format:** `HOST_PORT:CONTAINER_PORT`

**Example:**

```yaml
ports:
  - "5000:8080"
```

- **5000**: Port on your computer (host)
- **8080**: Port inside container
- **Result**: `localhost:5000` → container's port 8080

**Containers to Each Other:**

```yaml
# API connects to MySQL
Server=mysql;Port=3306 # ← Uses internal port 3306, not host port
```

- Containers communicate directly via internal ports
- No need to use host ports for inter-container communication

#### Network Diagram

```
Host (Your Computer)
├─ localhost:5000 ──┐
├─ localhost:8080 ──┤
│                   │
│  Docker Network (app-network)
│  ├─ mysql:3306 ────────┐
│  ├─ api:8080 ──────────┤
│  └─ phpmyadmin:80 ──────┘
│           │
│      (Communicate using service names)
│
└─ mysql-data volume (persistent storage)
```

### Data Persistence (Volumes)

#### What Are Volumes?

**Definition:**

- Storage outside container filesystem
- Survives container restarts and deletions
- Managed by Docker

**Why Needed?**

- **Container Filesystem is Temporary**: Deleted when container is removed
- **Database Data Must Persist**: Can't lose data on restart

#### Volume Types

**Named Volume (Used in This Project):**

```yaml
volumes: mysql-data:/var/lib/mysql
```

- **mysql-data**: Volume name (managed by Docker)
- **/var/lib/mysql**: Path inside container (MySQL's data directory)

**Where Is Data Stored?**

```bash
# Linux/Mac
/var/lib/docker/volumes/migratingassistant_mysql-data/_data

# Windows (Docker Desktop)
\\wsl$\docker-desktop-data\data\docker\volumes\migratingassistant_mysql-data\_data
```

#### Volume Lifecycle

**Container Actions vs Data:**

| Command                  | Containers   | Volumes        |
| ------------------------ | ------------ | -------------- |
| `docker-compose down`    | ✅ Removed   | ✅ **Kept**    |
| `docker-compose down -v` | ✅ Removed   | ❌ **Deleted** |
| `docker-compose rm`      | ✅ Removed   | ✅ **Kept**    |
| `docker-compose up`      | ✅ Recreated | ✅ **Reused**  |

**Safe Commands:**

```bash
# Stop containers, keep data
docker-compose down

# Restart with existing data
docker-compose up -d
```

**Dangerous Commands:**

```bash
# DELETE ALL DATA!
docker-compose down -v

# Remove specific volume
docker volume rm migratingassistant_mysql-data
```

#### Backup and Restore

**Backup MySQL Data:**

```bash
# Method 1: SQL dump (recommended)
docker-compose exec mysql mysqldump -u root -p MigratingAssistant > backup.sql

# Method 2: Copy volume (advanced)
docker run --rm -v migratingassistant_mysql-data:/data -v $(pwd):/backup ubuntu tar czf /backup/mysql-backup.tar.gz /data
```

**Restore MySQL Data:**

```bash
# Method 1: From SQL dump
docker-compose exec -T mysql mysql -u root -p MigratingAssistant < backup.sql

# Method 2: From volume backup
docker run --rm -v migratingassistant_mysql-data:/data -v $(pwd):/backup ubuntu tar xzf /backup/mysql-backup.tar.gz -C /
```

### Common Docker Questions

**Q: Do I need to install .NET on my computer?**  
**A:** No! .NET is inside the Docker container. You only need Docker.

**Q: Where is my data stored?**  
**A:** Database data is in Docker volume `mysql-data`. Managed by Docker automatically.

**Q: Can I access MySQL from outside Docker?**  
**A:** Yes in development (`localhost:3306`). No in production (security).

**Q: What happens if I restart my computer?**  
**A:** Containers stop but data persists. Run `docker-compose up -d` to start again.

**Q: How do I see what's inside a container?**  
**A:**

```bash
# Open shell in running container
docker-compose exec api bash

# Explore filesystem
ls -la
pwd
env
```

**Q: How do I update the application?**  
**A:**

```bash
# Pull latest code (if using Git)
git pull

# Rebuild and restart
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

**Q: Can I run this on a server?**  
**A:** Yes! Use `docker-compose.prod.yml`:

```bash
docker-compose -f docker-compose.prod.yml up -d
```

**Q: How do I reset everything?**  
**A:**

```bash
# Stop and remove containers, networks, and volumes
docker-compose down -v

# Remove images
docker-compose down --rmi all

# Start fresh
docker-compose up -d
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
