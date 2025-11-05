#!/bin/bash
set -e

echo "========================================"
echo "MigratingAssistant - Starting Application"
echo "========================================"

# Wait for MySQL to be ready
echo "Waiting for MySQL to be ready..."
max_attempts=30
attempt=0

# Function to check MySQL connectivity using timeout and simple connection test
check_mysql() {
    timeout 2 bash -c "</dev/tcp/mysql/3306" 2>/dev/null
    return $?
}

until check_mysql; do
  attempt=$((attempt + 1))
  if [ $attempt -ge $max_attempts ]; then
    echo "MySQL did not become ready in time after $max_attempts attempts"
    echo "Starting application anyway..."
    break
  fi
  echo "MySQL is unavailable - sleeping (attempt $attempt/$max_attempts)"
  sleep 2
done

if check_mysql; then
  echo "MySQL is up and accepting connections!"
  echo "Waiting additional 5 seconds for MySQL to be fully ready..."
  sleep 5
  echo "MySQL connection successful!"
else
  echo "Warning: MySQL not available, but continuing..."
fi

echo "========================================"
echo "Starting .NET Application..."
echo "========================================"

# Note: Migrations are handled by the application on startup
# See: Infrastructure/Data/ApplicationDbContextInitialiser.cs

# Find the DLL to run
if [ -f "MigratingAssistant.Web.dll" ]; then
    DLL_NAME="MigratingAssistant.Web.dll"
elif [ -f "Web.dll" ]; then
    DLL_NAME="Web.dll"
else
    echo "ERROR: Cannot find application DLL!"
    ls -la /app
    exit 1
fi

echo "Starting application: $DLL_NAME"

# Start the application
exec dotnet "$DLL_NAME"
