using Microsoft.Extensions.DependencyInjection; 
using PBL3.Core;
using PBL3.Data;
using PBL3.GUI;
using PBL3.Interface; 
using PBL3.Service;
using PBL3.Models;
using System;
using System.Windows;

namespace PBL3
{
    internal class Program
    {
        //DI toàn cục để các UserControl có thể gọi ké
        public static IServiceProvider ServiceProvider { get; private set; }

        [STAThread]
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            // --- 1. Đăng ký Database Context ---
            services.AddDbContext<MilkTeaDBContext>();

            // --- 2. Đăng ký TẤT CẢ các Service ---
            services.AddTransient<IIngredientService, IngredientService>();
            services.AddTransient<IItemService, ItemService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IStaffService, StaffService>();
            services.AddTransient<IProfitService, ProfitService>();
            services.AddTransient<IRevenueService, RevenueService>();
            services.AddTransient<IRecipeService, RecipeService>();
            services.AddTransient<IPasswordAuthenticator, UserAuthenticator>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IImportService, ImportService>();
            services.AddTransient<IReportService, ReportService>();
            services.AddTransient<ICustomerPointService, CustomerPointService>();
            services.AddTransient<ICustomerService, CustomerService>();

            // --- 3. Đăng ký các Cửa sổ (Window) ---
            services.AddTransient<wDangNhap>();
            services.AddTransient<wTrangChu_Boss>();
            services.AddTransient<wTrangChu_NhanVien>();
            services.AddTransient<wDangKy>();

            // --- 4. Chốt danh sách và tạo "Trung tâm phân phối" ---
            ServiceProvider = services.BuildServiceProvider();

            // --- 5. Khởi chạy ứng dụng ---
            System.Windows.Application app = new System.Windows.Application();

            // Lấy form Đăng nhập từ DI (thay thế cho chữ 'new' cũ)
            var loginWindow = ServiceProvider.GetRequiredService<wDangNhap>();

            app.Run(loginWindow);
        }
    }
}