# CarRental - Docker Database Setup

## Mục lục
1. [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
2. [Cài đặt và chạy](#cài-đặt-và-chạy)
3. [Cấu trúc thư mục](#cấu-trúc-thư-mục)
4. [Các lệnh Docker hữu ích](#các-lệnh-docker-hữu-ích)
5. [Kết nối database](#kết-nối-database)
6. [Troubleshooting](#troubleshooting)

## Yêu cầu hệ thống

- Docker Desktop (Windows/Mac) hoặc Docker Engine (Linux)
- Docker Compose
- .NET 8.0 SDK (để chạy ứng dụng local)
- Git

## Cài đặt và chạy

### Bước 1: Clone repository
```bash
git clone <repository-url>
cd CarRental
```

### Bước 2: Chạy database với Docker

#### Option A: Chỉ chạy database (khuyến nghị cho development)
```bash
# Chạy trực tiếp
docker-compose up postgres -d

# Hoặc sử dụng script tiện ích
# Windows:
scripts/start-db.bat

# Linux/Mac:
chmod +x scripts/start-db.sh
./scripts/start-db.sh
```

#### Option B: Chạy toàn bộ ứng dụng trong Docker
```bash
# Uncomment phần carrental-api trong docker-compose.yml trước
docker-compose up -d
```

### Bước 3: Chạy .NET application (nếu chọn Option A)
```bash
cd CarRental
dotnet restore
dotnet run
```

## Cấu trúc thư mục

```
CarRental/
├── docker-compose.yml              # Cấu hình Docker services
├── Dockerfile                      # Build image cho .NET app
├── .dockerignore                   # Loại bỏ files không cần thiết
├── database/
│   ├── init/
│   │   ├── 01-init-schema.sql     # Tạo schema và tables
│   │   └── 02-seed-data.sql       # Dữ liệu mẫu
│   └── backup/                     # Thư mục backup (tùy chọn)
├── scripts/
│   ├── start-db.bat               # Script Windows
│   └── start-db.sh                # Script Linux/Mac
└── CarRental/
    ├── appsettings.Docker.json    # Cấu hình cho Docker environment
    └── ...
```

## Kết nối database

### Connection String cho development
```
Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=Chich1412
```

### Thông tin database
- **Host**: localhost (local development) hoặc postgres (trong Docker network)
- **Port**: 5432
- **Database**: postgres
- **Username**: postgres
- **Password**: Chich1412
- **Schema**: car_rental_official3

### Database Schema
Database bao gồm các bảng chính:
- **roles**: Quản lý vai trò người dùng
- **companies**: Thông tin công ty/chi nhánh
- **users**: Thông tin người dùng hệ thống
- **refresh_tokens**: Quản lý JWT refresh tokens
- **vehicles**: Danh sách xe cho thuê
- **vehicle_status_logs**: Lịch sử thay đổi trạng thái xe
- **vehicle_pricing_rules**: Quy tắc định giá cho từng xe
- **bookings**: Quản lý đặt xe
- **maintenance**: Lịch bảo trì xe
- **maintenance_logs**: Nhật ký hoạt động bảo trì

### Tài khoản mặc định
- **Admin**: admin@carrental.com / Admin123!
- **Manager**: manager1@carrental.com / Manager123!
- **Employee**: employee1@carrental.com / Employee123!

### Dữ liệu mẫu
- 3 xe mẫu với biển số: 30A-12345, 30A-67890, 29A-11111
- Quy tắc định giá cho từng xe (800k-900k VNĐ/ngày)
- 1 lịch bảo trì mẫu

## Các lệnh Docker hữu ích

### Quản lý services
```bash
# Chạy tất cả services
docker-compose up -d

# Chỉ chạy database
docker-compose up postgres -d

# Dừng tất cả services
docker-compose down

# Dừng và xóa volumes (⚠️ sẽ mất dữ liệu)
docker-compose down -v

# Xem logs
docker-compose logs postgres
docker-compose logs carrental-api

# Theo dõi logs realtime
docker-compose logs -f postgres
```

### Quản lý database
```bash
# Kết nối vào PostgreSQL container
docker-compose exec postgres psql -U postgres -d postgres

# Backup database
docker-compose exec postgres pg_dump -U postgres postgres > backup.sql

# Restore database
docker-compose exec -T postgres psql -U postgres -d postgres < backup.sql

# Kiểm tra trạng thái database
docker-compose exec postgres pg_isready -U postgres
```

### Quản lý images và containers
```bash
# Xem containers đang chạy
docker-compose ps

# Rebuild image khi có thay đổi code
docker-compose build carrental-api
docker-compose up carrental-api -d

# Xóa tất cả containers và images (cleanup hoàn toàn)
docker-compose down --rmi all -v
```

## Environment Variables

Bạn có thể tạo file `.env` để override các biến môi trường:

```env
# .env file
POSTGRES_DB=postgres
POSTGRES_USER=postgres
POSTGRES_PASSWORD=YourSecurePassword
POSTGRES_PORT=5432
API_PORT=5000
API_PORT_SSL=5001
```

## Troubleshooting

### Lỗi thường gặp

1. **Port 5432 đã được sử dụng**
   ```bash
   # Kiểm tra process đang dùng port
   netstat -ano | findstr :5432  # Windows
   lsof -i :5432                 # Linux/Mac
   
   # Đổi port trong docker-compose.yml
   ports:
     - "5433:5432"  # Dùng port 5433 thay vì 5432
   ```

2. **Database không khởi động được**
   ```bash
   # Xem logs chi tiết
   docker-compose logs postgres
   
   # Xóa volume và tạo lại
   docker-compose down -v
   docker-compose up postgres -d
   ```

3. **Connection refused**
   ```bash
   # Kiểm tra container có chạy không
   docker-compose ps
   
   # Kiểm tra health check
   docker-compose exec postgres pg_isready -U postgres
   ```

4. **Permission denied on Windows**
   ```bash
   # Chạy PowerShell as Administrator
   Set-ExecutionPolicy RemoteSigned
   ```

### Kiểm tra kết nối

```bash
# Test connection từ host machine
docker-compose exec postgres psql -U postgres -c "SELECT version();"

# Test connection từ .NET app
# Xem logs của API container
docker-compose logs carrental-api
```

## Development Workflow

1. **Lần đầu setup**:
   ```bash
   git clone <repo>
   cd CarRental
   docker-compose up postgres -d
   cd CarRental
   dotnet run
   ```

2. **Hàng ngày**:
   ```bash
   docker-compose start postgres  # Nếu đã tạo container
   cd CarRental
   dotnet run
   ```

3. **Khi thay đổi database schema**:
   ```bash
   # Update migration files
   dotnet ef migrations add <MigrationName>
   dotnet ef database update
   
   # Hoặc rebuild container để apply init scripts
   docker-compose down postgres
   docker volume rm carrental_postgres_data
   docker-compose up postgres -d
   ```

## Tips cho Senior Developer

1. **Production Setup**: Thay đổi mật khẩu và secrets trong production
2. **Backup Strategy**: Setup automated backup cho production database
3. **Monitoring**: Thêm health checks và monitoring cho containers
4. **Security**: Sử dụng Docker secrets cho sensitive data
5. **Performance**: Tune PostgreSQL configuration cho production workload

---

**Lưu ý**: File này được tạo để hỗ trợ development team. Đối với production deployment, cần có thêm các cấu hình về security, monitoring, và backup.