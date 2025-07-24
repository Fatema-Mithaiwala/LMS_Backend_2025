using library_management_system_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Application.Configurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            builder.HasKey(w => w.WishlistId);
            builder.Property(w => w.IsNotified).HasDefaultValue(false);
            builder.Property(w => w.AddedAt).HasDefaultValueSql("GETDATE()");

            builder.HasOne(w => w.User)
                   .WithMany(u => u.Wishlist)
                   .HasForeignKey(w => w.UserId);

            builder.HasOne(w => w.Book)
                   .WithMany(b => b.Wishlist)
                   .HasForeignKey(w => w.BookId);
        }
    }
}
