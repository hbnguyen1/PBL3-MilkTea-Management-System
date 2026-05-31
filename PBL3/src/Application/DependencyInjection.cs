using Microsoft.Extensions.DependencyInjection;
using PBL3.src.Application.Interface;
using PBL3.src.Application.Service;

namespace PBL3.src.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Đăng ký TẤT CẢ các Service nghiệp vụ
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

            return services;
        }
    }
}