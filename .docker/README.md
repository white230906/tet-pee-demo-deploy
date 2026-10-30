# TetPee Docker

Thực hiện các bước dưới đây bằng PowerShell.

## 1. Yêu cầu

- Git
- Docker Desktop đang chạy

## 2. Clone source code

```powershell
git clone git@github.com:white230906/tet-pee-demo-deploy.git
```

```powershell
cd TetPee
```

## 3. Đăng nhập Docker Hub nếu repository image là private

```powershell
docker login
```

Nếu image là public, có thể bỏ qua bước này.

## 4. Tạo file cấu hình local

```powershell
Copy-Item .docker\.env.sample .docker\.env
```

```powershell
notepad .docker\.env
```

Thay `CHANGE_ME` bằng mật khẩu PostgreSQL thật. Không commit file `.env`.

## 5. Kiểm tra cấu hình Compose

```powershell
docker compose -p tetpee --env-file .docker/.env -f .docker/docker-compose.yml config -q
```

Lệnh không in lỗi nghĩa là cấu hình hợp lệ.

## 6. Tải image

```powershell
docker compose -p tetpee --env-file .docker/.env -f .docker/docker-compose.yml pull
```

## 7. Khởi động API và PostgreSQL

```powershell
docker compose -p tetpee --env-file .docker/.env -f .docker/docker-compose.yml up -d
```

## 8. Tạo database schema bằng migration

Lệnh này dùng .NET SDK tạm thời trong container; máy host không cần cài .NET SDK.

```powershell
docker run --rm --network tetpee_tetpee-network --env-file .docker/.env --mount "type=bind,source=$($PWD.Path),target=/src" -w /src mcr.microsoft.com/dotnet/sdk:8.0 bash -lc 'dotnet tool install --tool-path /tmp/tools dotnet-ef --version 8.0.0 && export ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" && /tmp/tools/dotnet-ef database update --project TetPee.Repository --startup-project TetPee.Api --context AppDbContext'
```

## 9. Khởi động lại API

```powershell
docker compose -p tetpee --env-file .docker/.env -f .docker/docker-compose.yml restart api
```

## 10. Kiểm tra container

```powershell
docker compose -p tetpee --env-file .docker/.env -f .docker/docker-compose.yml ps
```

PostgreSQL cần có trạng thái `healthy`; API cần có trạng thái `running`.

## 11. Mở Swagger

```text
http://localhost:8080/swagger/index.html
```

## 12. Xem log API

```powershell
docker compose -p tetpee --env-file .docker/.env -f .docker/docker-compose.yml logs -f api
```

Nhấn `Ctrl+C` để thoát màn hình log; container vẫn tiếp tục chạy.

## 13. Dừng hệ thống

```powershell
docker compose -p tetpee --env-file .docker/.env -f .docker/docker-compose.yml down
```

Lệnh này giữ lại volume và dữ liệu PostgreSQL.

## 14. Xóa cả hệ thống và dữ liệu PostgreSQL

> Cảnh báo: lệnh sau xóa volume và toàn bộ dữ liệu database.

```powershell
docker compose -p tetpee --env-file .docker/.env -f .docker/docker-compose.yml down -v
```
