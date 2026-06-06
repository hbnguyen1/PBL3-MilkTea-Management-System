using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBL3.src.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.src.Infrastructure.Data.Configurations
{
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder) => builder.ToTable("ADMIN");
    }
}
