using Microsoft.Extensions.DependencyInjection;
using PBL3.UI.Views;
using System;
using System.Windows;
// Nhớ using 2 thư mục này để gọi hàm mở rộng
using PBL3.src.Application;
using PBL3.src.Infrastructure;

namespace PBL3.UI
{
    internal class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread]
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            // 1. Gọi các gói DI đã được đóng gói sẵn
            services.AddApplicationServices();
            services.AddInfrastructureServices();

            //2. Đăng ký các Cửa sổ (Window) của tầng UI
            services.AddTransient<wDangNhap>();
            services.AddTransient<wTrangChu_Boss>();
            services.AddTransient<wTrangChu_NhanVien>();
            services.AddTransient<wDangKy>();
            services.AddTransient<wThemNhanVien>();

            //3. Chốt danh sách và tạo Trung tâm phân phối
            ServiceProvider = services.BuildServiceProvider();

            //4. Khởi chạy ứng dụng
            System.Windows.Application app = new System.Windows.Application();
            var loginWindow = ServiceProvider.GetRequiredService<wDangNhap>();
            app.Run(loginWindow);
        }
    }
}