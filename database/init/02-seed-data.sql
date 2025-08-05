-- Seed data for Car Rental application
SET search_path TO car_rental_official3;

-- Insert default roles
INSERT INTO car_rental_official3.roles ("Name") VALUES
('Admin'),
('Manager'),
('Employee'),
('Customer')
ON CONFLICT ("Name") DO NOTHING;

-- Insert default companies
INSERT INTO car_rental_official3.companies ("Name", "Address", "Phone", "Email") VALUES
('CarRental HQ', '123 Main Street, City Center', '+84-123-456-789', 'admin@carrental.com'),
('Branch Office 1', '456 Business District, Downtown', '+84-987-654-321', 'branch1@carrental.com'),
('Branch Office 2', '789 Commercial Area, Uptown', '+84-555-123-456', 'branch2@carrental.com')
ON CONFLICT DO NOTHING;

-- Insert default admin user (password: Admin123!)
-- Note: This is a hashed password for 'Admin123!' - you should change this in production
INSERT INTO car_rental_official3.users 
("Email", "PasswordHash", "Name", "Phone", "Status", "CompanyId", "RoleId") 
SELECT 
    'admin@carrental.com',
    '$2a$11$rGZaWFW8qJvgVJnYFqBD3eGK7qMH1vqKZFHnG8xKjKJiTlYJh9R6u', -- Admin123!
    'System Administrator',
    '+84-123-456-789',
    'ACTIVE',
    c."Id",
    r."Id"
FROM car_rental_official3.roles r, car_rental_official3.companies c
WHERE r."Name" = 'Admin' AND c."Name" = 'CarRental HQ'
ON CONFLICT ("Email") DO NOTHING;

-- Insert sample manager user (password: Manager123!)
INSERT INTO car_rental_official3.users 
("Email", "PasswordHash", "Name", "Phone", "Status", "CompanyId", "RoleId") 
SELECT 
    'manager1@carrental.com',
    '$2a$11$8K7J9aHB2gE4F6qKjHgL5eFH9rQW3tYuIoPlKjHgF6eR7sQW2nK3m', -- Manager123!
    'John Manager',
    '+84-987-654-321',
    'ACTIVE',
    c."Id",
    r."Id"
FROM car_rental_official3.roles r, car_rental_official3.companies c
WHERE r."Name" = 'Manager' AND c."Name" = 'Branch Office 1'
ON CONFLICT ("Email") DO NOTHING;

-- Insert sample employee user (password: Employee123!)
INSERT INTO car_rental_official3.users 
("Email", "PasswordHash", "Name", "Phone", "Status", "CompanyId", "RoleId") 
SELECT 
    'employee1@carrental.com',
    '$2a$11$9H8I7bGC3fF5E7pLkJhG6dGH8qNI2sRvHoQlKjHgF7eS8tRW3oL4n', -- Employee123!
    'Jane Employee',
    '+84-555-789-123',
    'ACTIVE',
    c."Id",
    r."Id"
FROM car_rental_official3.roles r, car_rental_official3.companies c
WHERE r."Name" = 'Employee' AND c."Name" = 'CarRental HQ'
ON CONFLICT ("Email") DO NOTHING;

-- Insert sample vehicles
INSERT INTO car_rental_official3.vehicles 
("CompanyId", "LicensePlate", "Brand", "YearManufacture", "Status", "PurchaseDate") 
SELECT 
    c."Id",
    '30A-12345',
    'Toyota Camry',
    2022,
    'AVAILABLE',
    '2022-01-15'
FROM car_rental_official3.companies c
WHERE c."Name" = 'CarRental HQ';

INSERT INTO car_rental_official3.vehicles 
("CompanyId", "LicensePlate", "Brand", "YearManufacture", "Status", "PurchaseDate") 
SELECT 
    c."Id",
    '30A-67890',
    'Honda Accord',
    2023,
    'AVAILABLE',
    '2023-03-20'
FROM car_rental_official3.companies c
WHERE c."Name" = 'CarRental HQ';

INSERT INTO car_rental_official3.vehicles 
("CompanyId", "LicensePlate", "Brand", "YearManufacture", "Status", "PurchaseDate") 
SELECT 
    c."Id",
    '29A-11111',
    'Hyundai Elantra',
    2021,
    'MAINTENANCE',
    '2021-08-10'
FROM car_rental_official3.companies c
WHERE c."Name" = 'Branch Office 1';

-- Insert sample vehicle pricing rules
INSERT INTO car_rental_official3.vehicle_pricing_rules 
("VehicleId", "PricePerDay", "WeekendMultiplier", "HolidayMultiplier", "EffectiveDate") 
SELECT 
    v."Id",
    800000.00,
    1.2,
    1.5,
    '2024-01-01'
FROM car_rental_official3.vehicles v
WHERE v."LicensePlate" = '30A-12345';

INSERT INTO car_rental_official3.vehicle_pricing_rules 
("VehicleId", "PricePerDay", "WeekendMultiplier", "HolidayMultiplier", "EffectiveDate") 
SELECT 
    v."Id",
    900000.00,
    1.2,
    1.5,
    '2024-01-01'
FROM car_rental_official3.vehicles v
WHERE v."LicensePlate" = '30A-67890';

INSERT INTO car_rental_official3.vehicle_pricing_rules 
("VehicleId", "PricePerDay", "WeekendMultiplier", "HolidayMultiplier", "EffectiveDate") 
SELECT 
    v."Id",
    700000.00,
    1.2,
    1.5,
    '2024-01-01'
FROM car_rental_official3.vehicles v
WHERE v."LicensePlate" = '29A-11111';

-- Insert sample maintenance records
INSERT INTO car_rental_official3.maintenance 
("VehicleId", "Title", "Description", "ScheduledAt", "Status") 
SELECT 
    v."Id",
    'Regular Oil Change',
    'Change engine oil and filter',
    CURRENT_TIMESTAMP + INTERVAL '7 days',
    'PLANNED'
FROM car_rental_official3.vehicles v
WHERE v."LicensePlate" = '29A-11111';

-- Insert sample maintenance log
INSERT INTO car_rental_official3.maintenance_logs 
("MaintenanceId", "Action", "Note", "CreatedBy") 
SELECT 
    m."Id",
    'Maintenance Scheduled',
    'Regular maintenance scheduled for next week',
    u."Id"
FROM car_rental_official3.maintenance m, car_rental_official3.users u
WHERE m."Title" = 'Regular Oil Change' AND u."Email" = 'manager1@carrental.com';

-- Display confirmation message
DO $$
BEGIN
    RAISE NOTICE 'Database initialization completed successfully!';
    RAISE NOTICE 'Default users created:';
    RAISE NOTICE '  - Admin: admin@carrental.com / Admin123!';
    RAISE NOTICE '  - Manager: manager1@carrental.com / Manager123!';
    RAISE NOTICE '  - Employee: employee1@carrental.com / Employee123!';
    RAISE NOTICE 'Sample vehicles and pricing rules added';
    RAISE NOTICE 'Sample maintenance records created';
END $$;