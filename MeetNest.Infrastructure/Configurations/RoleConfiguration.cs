using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeetNest.Domain.Entities;
using MeetNest.Domain.Enums;

namespace MeetNest.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.Name)
               .HasConversion<string>() // store enum as string
               .IsRequired();

        builder.Property(r => r.Description)
               .HasMaxLength(250);

        builder.Property(r => r.CreatedAt)
               .HasDefaultValueSql("NOW()"); // PostgreSQL timestamp default
    }
}
