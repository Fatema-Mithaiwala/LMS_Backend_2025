using library_management_system_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Application.Configurations
{
    public class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
    {
        public void Configure(EntityTypeBuilder<ReturnRequest> builder)
        {
            builder.HasKey(rr => rr.ReturnRequestId);
            builder.Property(rr => rr.ConditionRemarks).HasMaxLength(255);
            builder.Property(rr => rr.Status).IsRequired().HasMaxLength(20);
            builder.Property(rr => rr.PenaltyAmount).HasColumnType("decimal(10, 2)");

            builder.HasOne(rr => rr.User)
                   .WithMany(u => u.ReturnRequests)
                   .HasForeignKey(rr => rr.UserId);

            builder.HasOne(rr => rr.Book)
                   .WithMany(b => b.ReturnRequests)
                   .HasForeignKey(rr => rr.BookId);
        }
    }

}
