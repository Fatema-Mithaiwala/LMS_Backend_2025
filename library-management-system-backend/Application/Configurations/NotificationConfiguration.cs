using library_management_system_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Application.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.NotificationId);
            builder.Property(n => n.Title).IsRequired().HasMaxLength(100);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            builder.Property(n => n.NotificationType).IsRequired().HasMaxLength(50);
            builder.Property(n => n.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasOne(n => n.User)
                   .WithMany(u => u.Notifications)
                   .HasForeignKey(n => n.UserId);

            builder.HasOne(n => n.RelatedBook)
                   .WithMany(b => b.Notifications)
                   .HasForeignKey(n => n.RelatedBookId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
