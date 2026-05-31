using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBL3.src.Domain.Models;

public class ImportDetailConfiguration : IEntityTypeConfiguration<ImportDetail>
{
    public void Configure(EntityTypeBuilder<ImportDetail> builder)
    {
        builder.ToTable("IMPORT_DETAIL");
        builder.HasKey(id => new { id.importId, id.igId });
        builder.HasOne(d => d.ImportNote).WithMany(n => n.ImportDetails).HasForeignKey(d => d.importId);
        builder.HasOne(d => d.Ingredient).WithMany().HasForeignKey(d => d.igId);
    }
}