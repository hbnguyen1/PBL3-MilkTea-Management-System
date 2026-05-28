using Microsoft.Extensions.DependencyInjection;
using PBL3.src.Application.Interface;
using PBL3.src.Application.Service;
using PBL3.src.Infrastructure.Data;
using PBL3.UI.ViewModels;
using PBL3.UI.Views;
using System;
using System.Windows;

namespace PBL3.UI
{
    internal class Program
    {
        //DI toàn cục để các UserControl có thể gọi
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

            // --- 3. Đăng ký các ModelView ---
            services.AddTransient<LoginViewModel>();

            // --- 4. Đăng ký các Cửa sổ (Window) ---
            services.AddTransient<wDangNhap>();
            services.AddTransient<wTrangChu_Boss>();
            services.AddTransient<wTrangChu_NhanVien>();
            services.AddTransient<wDangKy>();
            services.AddTransient<wThemNhanVien>();

            // --- 5. Chốt danh sách và tạo Trung tâm phân phối ---
            ServiceProvider = services.BuildServiceProvider();

            // --- 6. Khởi chạy ứng dụng ---
            System.Windows.Application app = new System.Windows.Application();

            // Lấy form Đăng nhập từ DI 
            var loginWindow = ServiceProvider.GetRequiredService<wDangNhap>();

            app.Run(loginWindow);
        }
    }
}