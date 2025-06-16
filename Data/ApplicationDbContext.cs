using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShiftWiseAI.Server.Models;
using System.Reflection.Emit;
namespace ShiftWiseAI.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<ShiftAssignment> ShiftAssignments { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .Property(u => u.OrganizationId)
                .IsRequired();

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            builder.Entity<Organization>()
                .HasMany(o => o.Users)
                .WithOne(u => u.Organization)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Link Employee → Organization (employee belongs to one org)
            builder.Entity<Employee>()
                .HasOne(e => e.Organization)
                .WithMany(o => o.Employees)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ShiftAssignment>()
                .HasKey(sa => sa.Id);

            builder.Entity<ShiftAssignment>()
                .HasOne(sa => sa.Employee)
                .WithMany(e => e.AssignedShifts)
                .HasForeignKey(sa => sa.EmployeeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ShiftAssignment>()
                .HasOne(sa => sa.Shift)
                .WithMany(s => s.Assignments)
                .HasForeignKey(sa => sa.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
