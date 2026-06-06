using Microsoft.Extensions.DependencyInjection;
using PBL3.src.Infrastructure.Data;

namespace PBL3.src.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Đăng ký Database Context và các kết nối vật lý
            services.AddDbContext<MilkTeaDBContext>();
            return services;
        }
    }
}