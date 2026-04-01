# Hướng dẫn: Lưu trữ File trong CungLoi.API

## Tổng quan

Tài liệu này giải thích chi tiết cách hệ thống CungLoi.API lưu trữ file trong hai môi trường:
- **Development (Local)**: Lưu file trên disk server (trong thư mục `wwwroot/files`)
- **Production (Cloudinary)**: Lưu file trên Cloudinary cloud storage với CDN

## Mục lục

1. [**Kiến trúc hệ thống**](01-architecture.md) - Interface và cơ chế chọn Storage Provider
2. [**Local Storage**](02-local-storage.md) - Lưu file ở Local (Development)
3. [**Cloudinary Storage**](03-cloudinary-storage.md) - Lưu file trên Cloudinary (Production)
4. [**Setup Cloudinary**](04-cloudinary-setup.md) - Cấu hình và triển khai Cloudinary
5. [**Testing**](05-testing.md) - Testing & Verification
6. [**Migration**](06-migration.md) - Migration dữ liệu từ Local sang Cloudinary
7. [**So sánh**](07-comparison.md) - So sánh Local vs Cloudinary

## Quick Start

### Development
Không cần config gì, hệ thống tự động dùng Local Storage:
```bash
dotnet run
# File sẽ lưu trong wwwroot/files
```

### Production
Thêm config Cloudinary vào `appsettings.Production.json`:
```json
{
  "FileStorageSettings": {
    "Cloudinary": {
      "CloudName": "your-cloud-name",
      "ApiKey": "your-api-key",
      "ApiSecret": "your-api-secret"
    }
  }
}
```

## Liên hệ

Nếu có vấn đề, tham khảo từng phần chi tiết trong các file tương ứng.
