using library_management_system_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Application.Configurations
{
    public class GenreConfiguration : IEntityTypeConfiguration<Genre>
    {
        public void Configure(EntityTypeBuilder<Genre> builder)
        {
            builder.HasKey(g => g.GenreId);
            builder.Property(g => g.GenreName).IsRequired().HasMaxLength(50);
            builder.HasIndex(g => g.GenreName).IsUnique();
        }
    }
}
