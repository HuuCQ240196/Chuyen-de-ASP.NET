# ProLap - Website bán Laptop

## 1. Giới thiệu

**ProLap** là website bán laptop được xây dựng bằng **ASP.NET Core MVC**.

Website hỗ trợ các chức năng chính:

- Hiển thị danh sách sản phẩm
- Tìm kiếm sản phẩm
- Lọc sản phẩm theo thương hiệu
- Lọc sản phẩm theo danh mục
- Phân trang sản phẩm
- Xem thông tin chi tiết sản phẩm
- Hiển thị nhiều hình ảnh cho sản phẩm
- Thêm sản phẩm vào giỏ hàng
- Tăng, giảm và xóa sản phẩm trong giỏ hàng
- Đặt hàng
- Kiểm tra số lượng tồn kho
- Quản lý sản phẩm
- Quản lý thương hiệu
- Quản lý danh mục
- Quản lý đơn hàng
- Cập nhật trạng thái đơn hàng
- Hoàn lại tồn kho khi hủy đơn
- Dashboard thống kê
- Đăng nhập và phân quyền quản trị

---

# 2. Công nghệ sử dụng

Project sử dụng các công nghệ:

- ASP.NET Core MVC
- .NET 10.0
- Entity Framework Core 10.0.11
- ASP.NET Core Identity 10.0.11
- Microsoft SQL Server
- SQL Server Express
- Razor View
- HTML
- CSS
- Bootstrap
- JavaScript

---

# 3. Yêu cầu hệ thống

Để chạy project trên máy tính Windows, cần cài đặt:

### Visual Studio

Cài đặt Visual Studio và chọn workload:

**ASP.NET and web development**

Visual Studio cần hỗ trợ **.NET 10.0**.

### .NET SDK

Project sử dụng:

**.NET 10.0**

Có thể kiểm tra sau khi cài đặt bằng lệnh:

```bash
dotnet --version
```

### SQL Server

Có thể sử dụng:

**Microsoft SQL Server Express**

Project ban đầu được phát triển với SQL Server Express instance:

```text
.\SQLEXPRESS
```

### SQL Server Management Studio

Cài đặt **SQL Server Management Studio (SSMS)** để quản lý và khôi phục cơ sở dữ liệu.

---

# 4. Tải source code

Có thể tải project bằng Git.

Mở Command Prompt hoặc Git Bash và chạy:

```bash
git clone https://github.com/HuuCQ240196/Chuyen-de-ASP.NET.git
```

Sau khi clone hoàn tất:

```bash
cd Chuyen-de-ASP.NET
```

Hoặc có thể tải source trực tiếp từ GitHub bằng:

**Code → Download ZIP**

Sau đó giải nén file ZIP.

---

# 5. Cấu trúc project

Cấu trúc chính của repository:

```text
Chuyen-de-ASP.NET
│
├── Database
│   └── ProLapDB.bak
│
├── Image
│
├── Prolap
│   ├── Controllers
│   ├── data
│   ├── Migrations
│   ├── Models
│   ├── Properties
│   ├── Views
│   ├── wwwroot
│   ├── appsettings.json
│   ├── Program.cs
│   └── Prolap.csproj
│
├── .gitignore
├── Huong_dan_chay.txt
├── Prolap.slnx
└── README.md
```

---

# 6. Khôi phục cơ sở dữ liệu

Database của project có tên:

```text
ProLapDB
```

File backup được đặt tại:

```text
Database\ProLapDB.bak
```

## Các bước Restore Database

### Bước 1

Mở **SQL Server Management Studio (SSMS)**.

### Bước 2

Kết nối đến SQL Server.

Ví dụ nếu sử dụng SQL Server Express:

```text
Server name: .\SQLEXPRESS
Authentication: Windows Authentication
```

Sau đó chọn **Connect**.

### Bước 3

Trong Object Explorer:

Nhấp chuột phải vào:

```text
Databases
```

Chọn:

```text
Restore Database...
```

### Bước 4

Trong phần Source chọn:

```text
Device
```

Nhấn nút `...`

Chọn:

```text
Add
```

và tìm đến file:

```text
Database\ProLapDB.bak
```

### Bước 5

Chọn file backup và thực hiện **Restore**.

Sau khi hoàn tất, trong SSMS phải xuất hiện database:

```text
ProLapDB
```

---

# 7. Cấu hình kết nối Database

Mở file:

```text
Prolap\appsettings.json
```

Kiểm tra phần Connection String.

Server trong Connection String phải phù hợp với SQL Server trên máy đang chạy project.

Nếu sử dụng SQL Server Express với instance:

```text
.\SQLEXPRESS
```

thì giữ cấu hình tương ứng với `.\SQLEXPRESS`.

Nếu máy tính sử dụng SQL Server instance khác, cần thay đổi Server trong Connection String cho phù hợp.

Ví dụ:

```text
Server=.\SQLEXPRESS;Database=ProLapDB;Trusted_Connection=True;TrustServerCertificate=True;
```

> Không thay đổi tên database `ProLapDB` nếu đã restore database với tên này.

---

# 8. Mở project

Mở file:

```text
Prolap.slnx
```

bằng Visual Studio.

Hoặc mở trực tiếp:

```text
Prolap\Prolap.csproj
```

Visual Studio sẽ tải project.

Nếu được yêu cầu Restore NuGet Packages, hãy cho phép Visual Studio thực hiện restore.

Các package chính của project:

```text
Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.11
Microsoft.AspNetCore.Identity.UI 10.0.11
Microsoft.EntityFrameworkCore.SqlServer 10.0.11
Microsoft.EntityFrameworkCore.Tools 10.0.11
```

---

# 9. Build project

Trong Visual Studio chọn:

```text
Build
→ Build Solution
```

Hoặc sử dụng phím tắt:

```text
Ctrl + Shift + B
```

Đảm bảo kết quả hiển thị:

```text
Build succeeded
```

---

# 10. Chạy website

Trong Visual Studio nhấn:

```text
Ctrl + F5
```

hoặc nhấn nút Run trên thanh công cụ.

Visual Studio sẽ khởi động ASP.NET Core và mở website bằng trình duyệt.

---

# 11. Tài khoản quản trị

Project có tài khoản quản trị dùng để kiểm tra các chức năng Admin.

```text
Email: admin@prolap.com
Password: Admin123
```

Sau khi đăng nhập bằng tài khoản Admin, có thể sử dụng các chức năng quản trị như:

- Quản lý sản phẩm
- Quản lý thương hiệu
- Quản lý danh mục
- Quản lý đơn hàng
- Dashboard thống kê

> Tài khoản trên được cung cấp phục vụ mục đích học tập, kiểm thử và chấm đồ án.

---

# 12. Một số lỗi thường gặp

## Không kết nối được SQL Server

Kiểm tra SQL Server Express đã được khởi động hay chưa.

Kiểm tra Server name:

```text
.\SQLEXPRESS
```

Nếu máy sử dụng instance khác, sửa Connection String trong:

```text
Prolap\appsettings.json
```

---

## Không tìm thấy database ProLapDB

Mở SSMS và kiểm tra database:

```text
ProLapDB
```

Nếu chưa có, thực hiện Restore từ:

```text
Database\ProLapDB.bak
```

---

## Thiếu .NET SDK

Kiểm tra bằng:

```bash
dotnet --version
```

Project yêu cầu:

```text
.NET 10.0
```

Nếu máy chưa có .NET 10 SDK, cần cài đặt trước khi chạy project.

---

## NuGet Package bị thiếu

Trong Visual Studio:

```text
Tools
→ NuGet Package Manager
→ Package Manager Console
```

Có thể thực hiện restore hoặc sử dụng terminal:

```bash
dotnet restore
```

Sau đó Build lại project.

---

# 13. Chạy bằng Command Line

Ngoài Visual Studio, project cũng có thể được chạy bằng .NET CLI.

Di chuyển vào thư mục project:

```bash
cd Prolap
```

Restore package:

```bash
dotnet restore
```

Build:

```bash
dotnet build
```

Chạy:

```bash
dotnet run
```

Sau khi ứng dụng khởi động, terminal sẽ hiển thị địa chỉ localhost của website.

Mở địa chỉ đó bằng trình duyệt để sử dụng website.

---

# 14. Thông tin đồ án

**Tên đề tài:** Xây dựng website bán laptop bằng ASP.NET Core MVC

**Tên website:** ProLap

**Mục đích:** Đồ án môn học Chuyên Đề ASP.NET

**Repository:**  
https://github.com/HuuCQ240196/Chuyen-de-ASP.NET

---

## Lưu ý

Project được xây dựng phục vụ mục đích học tập.

Khi chạy project trên máy tính khác, hai thông tin quan trọng cần kiểm tra là:

1. Máy đã cài **.NET 10 SDK**.
2. Connection String trong `appsettings.json` trỏ đúng đến SQL Server chứa database `ProLapDB`.