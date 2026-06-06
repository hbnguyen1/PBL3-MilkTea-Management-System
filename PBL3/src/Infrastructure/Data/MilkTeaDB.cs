using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;
using PBL3.src.Domain.Models;
using System;

namespace PBL3.src.Infrastructure.Data
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
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
                string connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình Kế thừa (Inheritance)
            modelBuilder.Entity<Customer>().HasBaseType<Users>();
            modelBuilder.Entity<Staff>().HasBaseType<Users>();
            modelBuilder.Entity<Admin>().HasBaseType<Users>();

            // Nạp các file cấu hình ở folder Configurations vào
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MilkTeaDBContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}