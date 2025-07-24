using library_management_system_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Application.Configurations
{
    public class BorrowRequestConfiguration : IEntityTypeConfiguration<BorrowRequest>
    {
        public void Configure(EntityTypeBuilder<BorrowRequest> builder)
        {
            builder.HasKey(br => br.BorrowRequestId);
            builder.Property(br => br.Status).IsRequired().HasMaxLength(20);
            builder.Property(br => br.RequestDate).HasDefaultValueSql("GETDATE()");
            builder.Property(br => br.Remarks).HasMaxLength(255);

            builder.HasOne(br => br.User)
                   .WithMany(u => u.BorrowRequests)
                   .HasForeignKey(br => br.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(br => br.Book)
                   .WithMany(b => b.BorrowRequests)
                   .HasForeignKey(br => br.BookId);

            builder.HasOne(br => br.Approver)
                   .WithMany()
                   .HasForeignKey(br => br.ApprovedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(br => br.BorrowTransaction)
                   .WithOne(bt => bt.BorrowRequest)
                   .HasForeignKey<BorrowTransaction>(bt => bt.BorrowRequestId);
        }
    }
}
