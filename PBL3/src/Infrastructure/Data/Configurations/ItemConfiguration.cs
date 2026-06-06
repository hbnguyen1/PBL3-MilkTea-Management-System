using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBL3.src.Domain.Models;

namespace PBL3.src.Infrastructure.Data.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("ITEM");

            // Khóa chính
            builder.HasKey(i => new { i.itemID, i.size });

            // Bỏ qua thuộc tính không map
            builder.Ignore(i => i.FullImagePath);
        }
    }
}