using Microsoft.EntityFrameworkCore;
using Repository.Enum;
using Repository.Model;

namespace Repository
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
      : base(options)
        {
        }

        #region DbSet
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Company> Companies { get; set; }   
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Application> Applications { get; set; }

        #endregion

        #region Data seeding
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed roles
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { RoleId = 1, Name = Role.Admin.ToString() },
                new UserRole { RoleId = 2, Name = Role.Employer.ToString() },
                new UserRole { RoleId = 3, Name = Role.JobSeeker.ToString() }
            );

            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    FullName = "Admin User",
                    Email = "admin@gmail.com",
                    Password = "AQAAAAIAAYagAAAAEKUwcERKgsQD8okV3q1PXY0HHF8AnDuHvC337/jZs3WPvCfRpa39zuDBSQVMCSPOMQ==",
                    RoleId = 1,
                    Phone = "1234567890",
                    CreatedAt = new DateTime(2025, 7, 1, 6, 30, 0, DateTimeKind.Utc),
                    Active = true
                });

            // 👇 FIX: Prevent multiple cascade path problem
            modelBuilder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade); // allow cascade for Job

            modelBuilder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict); // restrict on User

            modelBuilder.Entity<Company>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // to avoid cycles with User

            modelBuilder.Entity<Job>()
                .Property(j => j.JobType)
                .HasConversion<string>();

            modelBuilder.Entity<Application>()
                .Property(a => a.Status)
                .HasConversion<string>();
        }

        #endregion

    }
}
