-- Create schema for Car Rental application
CREATE SCHEMA IF NOT EXISTS car_rental_official3;

-- Set search path to use the schema
SET search_path TO car_rental_official3;

-- Create roles table
CREATE TABLE IF NOT EXISTS car_rental_official3.roles (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL UNIQUE
);

-- Create companies table
CREATE TABLE IF NOT EXISTS car_rental_official3.companies (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "Address" TEXT,
    "Phone" VARCHAR(20),
    "Email" VARCHAR(255),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create users table
CREATE TABLE IF NOT EXISTS car_rental_official3.users (
    "Id" SERIAL PRIMARY KEY,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Name" VARCHAR(255),
    "Phone" VARCHAR(20),
    "Status" VARCHAR(50) DEFAULT 'ACTIVE',
    "CompanyId" INTEGER REFERENCES car_rental_official3.companies("Id") ON DELETE CASCADE,
    "RoleId" INTEGER REFERENCES car_rental_official3.roles("Id") ON DELETE CASCADE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create refresh_tokens table
CREATE TABLE IF NOT EXISTS car_rental_official3.refresh_tokens (
    "Id" SERIAL PRIMARY KEY,
    "UserId" INTEGER REFERENCES car_rental_official3.users("Id") ON DELETE CASCADE,
    "Token" VARCHAR(500) NOT NULL UNIQUE,
    "ExpiresAt" TIMESTAMP NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "RevokedAt" TIMESTAMP NULL
);

-- Create vehicles table
CREATE TABLE IF NOT EXISTS car_rental_official3.vehicles (
    "Id" SERIAL PRIMARY KEY,
    "CompanyId" INTEGER REFERENCES car_rental_official3.companies("Id") ON DELETE CASCADE,
    "LicensePlate" VARCHAR(20) NOT NULL UNIQUE,
    "Brand" VARCHAR(100),
    "YearManufacture" INTEGER,
    "Status" VARCHAR(50) DEFAULT 'AVAILABLE',
    "PurchaseDate" DATE,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create vehicle_status_logs table
CREATE TABLE IF NOT EXISTS car_rental_official3.vehicle_status_logs (
    "Id" SERIAL PRIMARY KEY,
    "VehicleId" INTEGER REFERENCES car_rental_official3.vehicles("Id") ON DELETE CASCADE,
    "OldStatus" VARCHAR(50),
    "NewStatus" VARCHAR(50),
    "ChangedBy" INTEGER REFERENCES car_rental_official3.users("Id") ON DELETE SET NULL,
    "ChangedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create vehicle_pricing_rules table
CREATE TABLE IF NOT EXISTS car_rental_official3.vehicle_pricing_rules (
    "Id" SERIAL PRIMARY KEY,
    "VehicleId" INTEGER REFERENCES car_rental_official3.vehicles("Id") ON DELETE CASCADE,
    "PricePerDay" DECIMAL(10,2) NOT NULL,
    "WeekendMultiplier" DECIMAL(3,2) DEFAULT 1.0,
    "HolidayMultiplier" DECIMAL(3,2) DEFAULT 1.0,
    "EffectiveDate" DATE NOT NULL,
    "ExpiryDate" DATE
);

-- Create bookings table
CREATE TABLE IF NOT EXISTS car_rental_official3.bookings (
    "Id" SERIAL PRIMARY KEY,
    "VehicleId" INTEGER REFERENCES car_rental_official3.vehicles("Id") ON DELETE CASCADE,
    "UserId" INTEGER REFERENCES car_rental_official3.users("Id") ON DELETE CASCADE,
    "StartDatetime" TIMESTAMP NOT NULL,
    "EndDatetime" TIMESTAMP NOT NULL,
    "Status" VARCHAR(50) DEFAULT 'PENDING',
    "TotalPrice" DECIMAL(12,2),
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create maintenance table
CREATE TABLE IF NOT EXISTS car_rental_official3.maintenance (
    "Id" SERIAL PRIMARY KEY,
    "VehicleId" INTEGER REFERENCES car_rental_official3.vehicles("Id") ON DELETE CASCADE,
    "Title" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "ScheduledAt" TIMESTAMP,
    "Status" VARCHAR(50) DEFAULT 'PLANNED',
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create maintenance_logs table
CREATE TABLE IF NOT EXISTS car_rental_official3.maintenance_logs (
    "Id" SERIAL PRIMARY KEY,
    "MaintenanceId" INTEGER REFERENCES car_rental_official3.maintenance("Id") ON DELETE CASCADE,
    "Action" VARCHAR(255) NOT NULL,
    "Note" TEXT,
    "CreatedBy" INTEGER REFERENCES car_rental_official3.users("Id") ON DELETE SET NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_users_email ON car_rental_official3.users("Email");
CREATE INDEX IF NOT EXISTS idx_users_company_id ON car_rental_official3.users("CompanyId");
CREATE INDEX IF NOT EXISTS idx_users_role_id ON car_rental_official3.users("RoleId");
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token ON car_rental_official3.refresh_tokens("Token");
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON car_rental_official3.refresh_tokens("UserId");
CREATE INDEX IF NOT EXISTS idx_vehicles_company_id ON car_rental_official3.vehicles("CompanyId");
CREATE INDEX IF NOT EXISTS idx_vehicles_license_plate ON car_rental_official3.vehicles("LicensePlate");
CREATE INDEX IF NOT EXISTS idx_vehicles_status ON car_rental_official3.vehicles("Status");
CREATE INDEX IF NOT EXISTS idx_vehicle_status_logs_vehicle_id ON car_rental_official3.vehicle_status_logs("VehicleId");
CREATE INDEX IF NOT EXISTS idx_vehicle_pricing_rules_vehicle_id ON car_rental_official3.vehicle_pricing_rules("VehicleId");
CREATE INDEX IF NOT EXISTS idx_vehicle_pricing_rules_effective_date ON car_rental_official3.vehicle_pricing_rules("EffectiveDate");
CREATE INDEX IF NOT EXISTS idx_bookings_vehicle_id ON car_rental_official3.bookings("VehicleId");
CREATE INDEX IF NOT EXISTS idx_bookings_user_id ON car_rental_official3.bookings("UserId");
CREATE INDEX IF NOT EXISTS idx_bookings_start_datetime ON car_rental_official3.bookings("StartDatetime");
CREATE INDEX IF NOT EXISTS idx_bookings_end_datetime ON car_rental_official3.bookings("EndDatetime");
CREATE INDEX IF NOT EXISTS idx_bookings_status ON car_rental_official3.bookings("Status");
CREATE INDEX IF NOT EXISTS idx_maintenance_vehicle_id ON car_rental_official3.maintenance("VehicleId");
CREATE INDEX IF NOT EXISTS idx_maintenance_status ON car_rental_official3.maintenance("Status");
CREATE INDEX IF NOT EXISTS idx_maintenance_logs_maintenance_id ON car_rental_official3.maintenance_logs("MaintenanceId");

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA car_rental_official3 TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA car_rental_official3 TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA car_rental_official3 TO postgres;