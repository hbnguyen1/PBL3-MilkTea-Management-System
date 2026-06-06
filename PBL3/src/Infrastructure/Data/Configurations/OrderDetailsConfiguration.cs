using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBL3.src.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.src.Infrastructure.Data.Configurations
{
    public class OrderDetailsConfiguration : IEntityTypeConfiguration<OrderDetails>
    {
        public void Configure(EntityTypeBuilder<OrderDetails> builder)
        {
            builder.ToTable("ORDERDETAILS");
            builder.HasKey(od => new { od.orderID, od.itemID, od.size });
            builder.HasOne(od => od.Order).WithMany(o => o.OrderDetails).HasForeignKey(od => od.orderID);
            builder.HasOne(od => od.Item).WithMany()
                   .HasForeignKey(od => new { od.itemID, od.size })
                   .HasPrincipalKey(i => new { i.itemID, i.size });
        }
    }
}
