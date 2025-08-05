#!/bin/bash

echo "Starting PostgreSQL database with Docker..."
docker-compose up postgres -d

echo "Waiting for database to be ready..."
sleep 10

echo "Database is ready!"
echo "You can now run your .NET application locally"
echo "Connection string: Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=Chich1412"

# Check if database is responding
if docker-compose exec postgres pg_isready -U postgres; then
    echo "✅ Database is healthy and ready to accept connections"
else
    echo "❌ Database is not responding. Please check Docker logs: docker-compose logs postgres"
fi