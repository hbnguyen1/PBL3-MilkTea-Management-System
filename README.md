# Hệ Thống Quản Lý Quán Trà Sữa (Milk Tea Shop Management) - Đồ án PBL3

Dự án phần mềm quản lý cửa hàng, tập trung vào việc tối ưu hóa luồng nghiệp vụ thực tế tại các quán trà sữa/cà phê, từ khâu quản lý kho nguyên liệu, tiếp nhận đơn hàng đến báo cáo doanh thu.

## Công Nghệ Sử Dụng
- Ngôn ngữ: C# (.NET 10)
- Cơ sở dữ liệu: SQL Server
- ORM: Entity Framework Core (Code-First / Database-First)

## Các Tính Năng Nổi Bật (Key Features)

1. Phân quyền Người Dùng Chặt Chẽ (Role-Based Access Control)
- Admin: Toàn quyền quản lý hệ thống, cấu hình Menu, xem báo cáo doanh thu.
- Staff (Nhân viên): Xử lý đơn hàng, đặt theo yêu cầu khách hàng.
- Customer (Khách hàng): Xem Menu, theo dõi lịch sử mua hàng cá nhân.

2. Quản Lý Món & Công Thức Thông Minh (Smart Recipe Management)
- Nguyên lý All-or-Nothing (Transaction): Thêm món mới bắt buộc đi kèm định mức nguyên liệu. Đảm bảo không có các món xuất hiện mà không có công thức đi kèm.
- Kế thừa Định lượng (Base Recipe): Hệ thống tự động kế thừa danh sách nguyên liệu từ Size M sang Size L, nhân viên quản lý chỉ cần nhập lại định lượng mới, tiết kiệm 50% thời gian thao tác.
- Tìm kiếm linh hoạt (Hybrid Search): Hỗ trợ tìm kiếm nguyên liệu bằng ID hoặc Tên (sử dụng *Fuzzy Search*).

3. Hệ Thống Xử Lý Đơn Hàng (KDS - Kitchen Display System)
- Ứng dụng cấu trúc dữ liệu Queue (Hàng đợi) để xử lý đơn hàng theo nguyên tắc FIFO (First In - First Out). //chưa triển khai
- Staff không được phép "chọn" đơn để làm, hệ thống tự động đẩy đơn cũ nhất lên màn hình, đảm bảo tính công bằng và thời gian chờ tối ưu cho khách.

4. Báo Cáo Thống Kê & Tối Ưu Hiệu Năng
- Hiển thị Top 5 món bán chạy nhất theo số lượng (Best Sellers) hỗ trợ ra quyết định nhập kho.
- Tối ưu hóa LINQ & RAM: Các phép tính cộng dồn (Sum), gom nhóm (GroupBy) được giao hoàn toàn cho SQL Server xử lý. Chỉ gọi ".ToList()" ở khâu cuối cùng.
- Bảo mật dữ liệu với DTO: Tránh hoàn toàn lỗi *Over-fetching*, chỉ gửi đúng những trường dữ liệu cần thiết (Tên món, Số lượng) ra màn hình của đối tượng được cấp quyền, ẩn giấu tuyệt đối các dữ liệu nhạy cảm (Doanh thu, Giá vốn).

## Hướng Dẫn Cài Đặt (Setup Instructions)

1. Clone repository này về máy.
2. Mở Terminal (CMD hoặc PowerShell) tại thư mục gốc của dự án (nơi vừa clone về).
3. Chạy lệnh sau để khởi động toàn bộ hệ thống (App + Database): `docker-compose up -d --build`
4. Mở Solution bằng Visual Studio.
5. Mở file `MilkTeaDBContext.cs` (hoặc `appsettings.json` nếu có) và cập nhật chuỗi kết nối `ConnectionString` cho phù hợp với SQL Server Local của bạn.
6. Mở Package Manager Console và chạy lệnh để khởi tạo Database:
   ```bash
   Update-Database
   ```
7. Chạy project.
