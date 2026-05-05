using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;
using PBL3.Models;

namespace PBL3.Data
{
    public class MilkTeaDBContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<ImportNote> ImportNotes { get; set; }
        public DbSet<ImportDetail> ImportDetails { get; set; }
        public DbSet<WorkShiftLog> WorkShiftLogs { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<SalarySummary> SalarySummaries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//<<<<<<< Updated upstream
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                string connectionString = configuration.GetConnectionString("DefaultConnection");
//=======
        //        string connectionString =
        //"Server=localhost,1433;Database=PBL3;User Id=sa;Password=PBL3_MilkTea@2026;TrustServerCertificate=True;";
//>>>>>>> Stashed changes
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //ÁNH XẠ TÊN BẢNG
            modelBuilder.Entity<Users>().ToTable("USERS");
            modelBuilder.Entity<Staff>().ToTable("STAFF");
            modelBuilder.Entity<Customer>().ToTable("CUSTOMER");
            modelBuilder.Entity<Item>().ToTable("ITEM");
            modelBuilder.Entity<Recipe>().ToTable("RECIPE");
            modelBuilder.Entity<Ingredient>().ToTable("INGREDIENT"); 
            modelBuilder.Entity<ImportNote>().ToTable("IMPORT_NOTE");
            modelBuilder.Entity<ImportDetail>().ToTable("IMPORT_DETAIL");
            modelBuilder.Entity<Orders>().ToTable("ORDERS"); 
            modelBuilder.Entity<OrderDetails>().ToTable("ORDERDETAILS");

            //CẤU HÌNH KHÓA CHÍNH (PRIMARY KEYS)
            modelBuilder.Entity<Users>().HasKey(u => u.userID);
            modelBuilder.Entity<Ingredient>().HasKey(i => i.igID);
            modelBuilder.Entity<Orders>().HasKey(o => o.orderID);
            modelBuilder.Entity<ImportNote>().HasKey(i => i.importID);

            // Khóa chính kép
            modelBuilder.Entity<Item>().HasKey(i => new { i.itemID, i.size });
            modelBuilder.Entity<OrderDetails>().HasKey(od => new { od.orderID, od.itemID, od.size });
            modelBuilder.Entity<Recipe>().HasKey(r => new { r.itemID, r.size, r.ingredientID });
            modelBuilder.Entity<ImportDetail>().HasKey(id => new { id.importId, id.igId });

            // CẤU HÌNH KHÓA NGOẠI (FOREIGN KEYS)

            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Ingredient)
                .WithMany()
                .HasForeignKey(r => r.ingredientID);

            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Item)
                .WithMany()
                .HasForeignKey(r => new { r.itemID, r.size });

            modelBuilder.Entity<ImportDetail>()
                .HasOne(d => d.ImportNote)
                .WithMany(n => n.ImportDetails) // 1 phiếu nhập có nhiều chi tiết
                .HasForeignKey(d => d.importId);


            modelBuilder.Entity<ImportDetail>()
                .HasOne(d => d.Ingredient)
                .WithMany() 
                .HasForeignKey(d => d.igId);

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<WorkShiftLog>().ToTable("WorkShiftLog");
        }
    }
}