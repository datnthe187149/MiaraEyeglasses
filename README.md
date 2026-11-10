# MIARA Eyeglasses — Hệ Thống Bán Kính Mắt Trực Tuyến

> Dự án website thương mại điện tử chuyên ngành kính mắt thời trang và kính thuốc, xây dựng trên nền tảng **ASP.NET Core 9.0 MVC** và **Microsoft SQL Server**.

---

## 🌟 Tính Năng Nổi Bật

1. **Phân hệ Sản phẩm & Kính mắt chuyên sâu (Optical Catalog):**
   - Quản lý danh mục gọng kính theo dáng (Vuông, Tròn, Cat-eye, Browline...) và chất liệu (Nhựa Acetate, Kim loại, Titan).
   - Biến thể sản phẩm (`ProductVariants`): Quản lý màu sắc, kích cỡ, tồn kho thực tế và mã SKU riêng.
   - Gói tròng mắt kính (`LensOptions`): Tròng cận/loạn, chống ánh sáng xanh, đổi màu khi ra nắng, chiết suất cao (1.60, 1.67, 1.74).

2. **Phân hệ Giỏ hàng & Mua hàng nhanh (Cart & Guest Checkout):**
   - Kết hợp gọng kính + biến thể màu sắc + gói tròng mắt kính trong 1 dòng sản phẩm.
   - Hỗ trợ **Guest Checkout** (mua hàng nhanh không bắt buộc đăng nhập) chỉ cần điền Họ tên, SĐT và Địa chỉ nhận hàng.
   - Áp dụng mã giảm giá Voucher (`Coupons`), tính toán tạm tính và tổng tiền tự động.

3. **Phân hệ Quản trị & Vận hành (Staff & Admin Portal):**
   - **Nhân viên (Staff):** Quản lý trạng thái đơn hàng, lưu tồn kho, kiểm duyệt đánh giá của khách hàng.
   - **Quản trị viên (Admin):** Quản lý CRUD Danh mục, Thương hiệu, Sản phẩm, Tài khoản người dùng, Khóa/Mở khóa tài khoản, Hoàn tiền đơn hàng.

4. **An toàn & Toàn vẹn dữ liệu (Data Integrity):**
   - Xử lý tạo đơn hàng, trừ kho, hoàn tồn kho qua **Stored Procedures** với Transaction an toàn.
   - Mã hóa mật khẩu bảo mật bằng **BCrypt** và băm token xác thực bằng **SHA-256**.

---

## 🛠️ Công Nghệ Sử Dụng

- **Backend:** C# (.NET 9.0), ASP.NET Core MVC
- **Database:** Microsoft SQL Server (T-SQL, Stored Procedures, Transactions)
- **Frontend:** Razor Pages / Views, HTML5, CSS3, JavaScript (thuần túy, modular)
- **Authentication:** Cookie Authentication kết hợp Claims-based authorization

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy Dự Án

### 1. Khởi tạo Cơ sở dữ liệu
- Mở **SQL Server Management Studio (SSMS)** hoặc **Azure Data Studio**.
- Mở file kịch bản SQL tại `scripts/database/setup_database.sql` (hoặc `database/setup_database.sql`).
- Chạy toàn bộ file để tự động khởi tạo database `EyeglassesShop`, các bảng, ràng buộc và nạp sẵn dữ liệu mẫu.

### 2. Cấu hình Chuỗi kết nối (Connection String)
Mở file `src/appsettings.json` và cập nhật thông tin máy chủ SQL Server của bạn:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=EyeglassesShop;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;"
}
```

### 3. Biên dịch và Chạy ứng dụng
Mở Terminal tại thư mục gốc của dự án:
```bash
# Phục hồi packages và build dự án
dotnet build src/MiaraEyeglasses.csproj

# Chạy ứng dụng
dotnet run --project src/MiaraEyeglasses.csproj
```
Truy cập trình duyệt tại: `http://localhost:5281` (hoặc cổng hiển thị trên console).

---

## 👥 Tài Khoản Mặc Định Thử Nghiệm

| Vai trò | Email | Mật khẩu | Quyền hạn |
| :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `admin@miara.vn` | `Admin@123` | Toàn quyền quản trị hệ thống (`/AdminDashboard`) |
| **Nhân viên (Staff)** | `staff@miara.vn` | `Staff@123` | Xử lý đơn hàng, kho hàng, đánh giá (`/Staff`) |
| **Khách hàng (Customer)** | `customer@miara.vn` | `Customer@123` | Mua sắm, theo dõi đơn hàng cá nhân |

