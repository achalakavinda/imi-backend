# Docker Quick Start Guide 🚀

This guide will help you get the MigratingAssistant platform running with Docker in under 5 minutes.

## Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose (included with Docker Desktop)

## Step 1: Setup Environment

```bash
# Clone the repository (if not already done)
# cd MigratingAssistant

# Copy environment template
cp .env.example .env

# (Optional) Edit .env to customize settings
# The default values work for development
```

## Step 2: Start the Application

```bash
# Build and start all containers
docker-compose up -d

# This will:
# ✅ Pull MySQL 8.0 image
# ✅ Build .NET application image
# ✅ Start MySQL database
# ✅ Run database migrations automatically
# ✅ Start the API
# ✅ Start phpMyAdmin for database management
```

## Step 3: Verify Everything is Running

```bash
# Check container status
docker ps

# You should see 3 containers running:
# - migratingassistant-api
# - migratingassistant-mysql
# - migratingassistant-phpmyadmin

# View API logs
docker-compose logs -f api
```

## Step 4: Access the Application

Open your browser and navigate to:

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health
- **phpMyAdmin**: http://localhost:8080
  - Server: `mysql`
  - Username: `root`
  - Password: `rootpassword`

## Step 5: Test the API

### Default Admin Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@migratingassistant.com",
    "password": "Admin@123456"
  }'
```

### Health Check

```bash
curl http://localhost:5000/health
```

## Common Commands

```bash
# View logs in real-time
docker-compose logs -f

# Stop all containers
docker-compose down

# Restart after code changes
docker-compose up --build

# Access MySQL CLI
docker-compose exec mysql mysql -u root -p

# Execute shell in API container
docker-compose exec api bash
```

## Troubleshooting

### Port Already in Use

```bash
# Change ports in .env file
echo "API_PORT=5001" >> .env
echo "MYSQL_PORT=3307" >> .env
echo "PHPMYADMIN_PORT=8081" >> .env

# Restart containers
docker-compose down
docker-compose up -d
```

### Container Won't Start

```bash
# Check logs for errors
docker-compose logs api
docker-compose logs mysql

# Rebuild without cache
docker-compose build --no-cache
docker-compose up -d
```

### Database Connection Errors

```bash
# Wait for MySQL to be fully ready (takes ~30 seconds)
docker-compose logs mysql | grep "ready for connections"

# Restart API after MySQL is ready
docker-compose restart api
```

### Reset Everything

```bash
# Stop and remove all containers and volumes (DELETES DATA!)
docker-compose down -v

# Start fresh
docker-compose up -d
```

## Next Steps

1. ✅ Change default admin password
2. ✅ Explore Swagger UI at http://localhost:5000/swagger
3. ✅ Review API endpoints in README.md
4. ✅ Start developing your immigration platform features

## Production Deployment

For production deployment, use:

```bash
# Copy and configure production environment
cp .env.example .env
# Edit .env with SECURE production values

# Start with production configuration
docker-compose -f docker-compose.prod.yml up -d
```

Refer to the main README.md for detailed production security checklist.

## Support

- **Documentation**: See README.md for complete documentation
- **Issues**: Report issues on GitHub
- **Docker Logs**: `docker-compose logs -f`

Happy coding! 🎉
