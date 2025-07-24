using library_management_system_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Application.Configurations
{
    public class BorrowTransactionConfiguration : IEntityTypeConfiguration<BorrowTransaction>
    {
        public void Configure(EntityTypeBuilder<BorrowTransaction> builder)
        {
            builder.HasKey(bt => bt.TransactionId);

            builder.Property(bt => bt.PenaltyAmount).HasColumnType("decimal(10, 2)");
            builder.Property(bt => bt.Notes).HasMaxLength(500);

            builder.HasOne(bt => bt.User)
                   .WithMany(u => u.BorrowTransactions)
                   .HasForeignKey(bt => bt.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(bt => bt.Book)
                   .WithMany(b => b.BorrowTransactions)
                   .HasForeignKey(bt => bt.BookId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(bt => bt.BorrowRequest)
                   .WithOne(br => br.BorrowTransaction)
                   .HasForeignKey<BorrowTransaction>(bt => bt.BorrowRequestId)
                   .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(bt => bt.ReturnRequest)
                   .WithOne(rr => rr.BorrowTransaction)
                   .HasForeignKey<ReturnRequest>(rr => rr.TransactionId)
                   .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
