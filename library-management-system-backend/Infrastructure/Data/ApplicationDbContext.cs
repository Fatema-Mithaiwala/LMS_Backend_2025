using library_management_system_backend.Application.Configurations;
using library_management_system_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }

        public DbSet<BorrowRequest> BorrowRequests { get; set; }
        public DbSet<BorrowTransaction> BorrowTransactions { get; set; }
        public DbSet<ReturnRequest> ReturnRequests { get; set; }
        public DbSet<Wishlist> WishLists { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new BookConfiguration());
            modelBuilder.ApplyConfiguration(new BorrowRequestConfiguration());
            modelBuilder.ApplyConfiguration(new BorrowTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new ReturnRequestConfiguration());
            modelBuilder.ApplyConfiguration(new WishlistConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
