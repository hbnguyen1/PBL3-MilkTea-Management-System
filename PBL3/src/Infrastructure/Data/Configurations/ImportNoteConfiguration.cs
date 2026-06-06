using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBL3.src.Domain.Models;

public class ImportNoteConfiguration : IEntityTypeConfiguration<ImportNote>
{
    public void Configure(EntityTypeBuilder<ImportNote> builder)
    {
        builder.ToTable("IMPORT_NOTE");
        builder.HasKey(i => i.importID);
        builder.HasOne(i => i.Staff).WithMany().HasForeignKey(i => i.staffID).HasPrincipalKey(s => s.userID); 
    }
}