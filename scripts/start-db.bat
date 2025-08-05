@echo off
echo Starting PostgreSQL database with Docker...
docker-compose up postgres -d

echo Waiting for database to be ready...
timeout /t 10

echo Database is ready!
echo You can now run your .NET application locally
echo Connection string: Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=Chich1412

pause