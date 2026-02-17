using IdentityDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCoreIdentityDemo.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRoles, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            //Renametable
            builder.Entity<ApplicationUser>().ToTable("USers");
            builder.Entity<ApplicationRoles>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            // Seed initial roles using HasData

            var adminRoleId = Guid.Parse("c8d89a25-4b96-4f20-9d79-7f8a54c5213d");
            var userRoleId = Guid.Parse("b92f0a3e-573b-4b12-8db1-2ccf6d58a34a");
            var managerRoleId = Guid.Parse("d7f4a42e-1c1b-4c9f-8a50-55f6b234e8e2");
            var guestRoleId = Guid.Parse("f2e6b8a1-9d43-4a7c-9f32-71d7c5dbe9f0");

            builder.Entity<ApplicationRoles>().HasData(
                new ApplicationRoles { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN", Description = "Administrator role with full permissions.", IsActive = true, CreatedOn = new DateTime(2025, 8, 4), ModifiedOn = new DateTime(2025, 8, 4) },
                new ApplicationRoles { Id = userRoleId, Name = "User", NormalizedName = "USER", Description = "Standard user role.", IsActive = true, CreatedOn = new DateTime(2025, 8, 4), ModifiedOn = new DateTime(2025, 8, 4) },
                new ApplicationRoles { Id = managerRoleId, Name = "Manager", NormalizedName = "MANAGER", Description = "Manager role with moderate permissions.", IsActive = true, CreatedOn = new DateTime(2025, 8, 4), ModifiedOn = new DateTime(2025, 8, 4) },
                new ApplicationRoles { Id = guestRoleId, Name = "Guest", NormalizedName = "GUEST", Description = "Guest role with limited access.", IsActive = true, CreatedOn = new DateTime(2025, 8, 4), ModifiedOn = new DateTime(2025, 8, 4) }
            );
        }
        public DbSet<Address> Addresses { get; set; }


    }
    }
