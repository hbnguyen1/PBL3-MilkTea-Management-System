using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBL3.src.Domain.Models;

public class SalarySummaryConfiguration : IEntityTypeConfiguration<SalarySummary>
{
    public void Configure(EntityTypeBuilder<SalarySummary> builder)
    {
        builder.ToTable("SalarySummary");
        builder.HasKey(s => s.id);
        builder.HasOne(s => s.Staff).WithMany().HasForeignKey(s => s.staffID).HasPrincipalKey(s => s.userID);
    }
}