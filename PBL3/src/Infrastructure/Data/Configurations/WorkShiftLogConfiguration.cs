using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBL3.src.Domain.Models;

public class WorkShiftLogConfiguration : IEntityTypeConfiguration<WorkShiftLog>
{
    public void Configure(EntityTypeBuilder<WorkShiftLog> builder)
    {
        builder.ToTable("WorkShiftLog");
        builder.HasKey(w => w.logID);
        builder.HasOne(w => w.Staff).WithMany(w => w.WorkShiftLogs).HasForeignKey(w => w.staffID).HasPrincipalKey(s => s.userID); ;
    }
}