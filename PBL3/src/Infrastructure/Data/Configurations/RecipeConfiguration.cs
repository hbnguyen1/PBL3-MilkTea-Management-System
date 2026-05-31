using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PBL3.src.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3.src.Infrastructure.Data.Configurations
{
    public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            builder.ToTable("RECIPE");
            builder.HasKey(r => r.recipeID);
            builder.HasOne(r => r.Ingredient).WithMany().HasForeignKey(r => r.ingredientID);
            builder.HasOne(r => r.Item).WithMany(i => i.Recipes)
                   .HasForeignKey(r => new { r.itemID, r.size })
                   .HasPrincipalKey(i => new { i.itemID, i.size });
        }
    }
}
