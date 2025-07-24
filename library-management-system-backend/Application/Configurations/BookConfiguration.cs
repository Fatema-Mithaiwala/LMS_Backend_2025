using library_management_system_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Application.Configurations
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.HasKey(b => b.BookId);
            builder.Property(b => b.Title).IsRequired().HasMaxLength(200);
            builder.Property(b => b.Author).IsRequired().HasMaxLength(100);
            builder.Property(b => b.ISBN).IsRequired().HasMaxLength(20);
            builder.Property(b => b.Description).HasMaxLength(1000);
            builder.Property(b => b.TotalCopies).IsRequired();
            builder.Property(b => b.AvailableCopies).IsRequired();

            builder.HasIndex(b => b.Title);
            builder.HasOne(b => b.Genre)
                   .WithMany(g => g.Books)
                   .HasForeignKey(b => b.GenreId);
        }
    }
}
