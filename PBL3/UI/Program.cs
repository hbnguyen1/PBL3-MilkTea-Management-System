using Microsoft.Extensions.DependencyInjection;
using PBL3.UI.Views;
using System;
using System.Windows;
using PBL3.src.Application;
using PBL3.src.Infrastructure;

namespace PBL3.UI
{
    internal class Program
    {
        //Biến static khai báo để làm kho chứa các service
        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread] //Single Thread Apartment: Bắt buộc giao diện UI phải chạy trên 1 luồng duy nhất
        static void Main(string[] args)
        {
            
            var services = new ServiceCollection(); //Tạo ra danh sách trống để đăng ký các Service

            //Gọi các gói DI đã được đóng gói sẵn
            services.AddApplicationServices(); //Lấy các đăng ký Service ở tầng Appication nạp vào
            services.AddInfrastructureServices(); //Lấy đăng ký Database Context ở tầng Infrastructure nạp vào

            //Đăng ký các Cửa sổ (Window) của tầng UI
            services.AddTransient<wDangNhap>(); 
            services.AddTransient<wTrangChu_Boss>();
            services.AddTransient<wTrangChu_NhanVien>();
            services.AddTransient<wDangKy>();
            services.AddTransient<wThemNhanVien>();

            //Chốt danh sách và tạo Trung tâm phân phối ServiceProvider để cấp các Service cho các tầng khác có thể xin cấp phát
            ServiceProvider = services.BuildServiceProvider();

            //Khởi chạy ứng dụng
            System.Windows.Application app = new System.Windows.Application();
            var loginWindow = ServiceProvider.GetRequiredService<wDangNhap>();
            app.Run(loginWindow);
        }
    }
}