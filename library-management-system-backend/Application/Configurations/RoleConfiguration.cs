using library_management_system_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace library_management_system_backend.Application.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.RoleId);
            builder.Property(r => r.RoleName).IsRequired().HasMaxLength(50);
            builder.HasIndex(r => r.RoleName).IsUnique();

            builder.HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Librarian" },
                new Role { RoleId = 3, RoleName = "Student" }
            );
        }
    }
}
