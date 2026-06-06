using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBL3.src.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.src.Infrastructure.Data.Configurations
{
    public class OrdersConfiguration : IEntityTypeConfiguration<Orders>
    {
        public void Configure(EntityTypeBuilder<Orders> builder)
        {
            builder.ToTable("ORDERS");
            builder.HasKey(o => o.orderID);
            builder.HasOne(o => o.Staff).WithMany(o => o.Orders).HasForeignKey(o => o.staffID).HasPrincipalKey(c => c.userID);
            builder.HasOne(o => o.Customer).WithMany().HasForeignKey(o => o.customerID).HasPrincipalKey(s => s.userID);
        }
    }
}
