using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeetNest.Domain.Entities;

namespace MeetNest.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(b => b.City)
               .IsRequired()
               .HasMaxLength(100);

        //  ADD UNIQUE CONSTRAINT HERE
        builder.HasIndex(b => new { b.Name, b.City })
               .IsUnique();
    }
}