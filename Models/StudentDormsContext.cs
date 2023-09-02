using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Windows.Controls;

namespace StudentDormsApp.Models;

public class StudentDormsContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["DbStudentDormsConnectionString"].ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>()
            .Ignore(s => s.FullName);
        modelBuilder.Entity<Student>()
            .Property(s => s.Type)
            .HasConversion<int>();

        modelBuilder.Entity<Dorm>()
            .Property(d => d.Tax)
            .HasPrecision(19, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.Amount)
            .HasPrecision(19, 2);
        modelBuilder.Entity<Invoice>()
            .Property(i => i.State)
            .HasConversion<int>();
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Student)
            .WithMany(s => s.Invoices)
            .HasForeignKey(i => i.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Room)
            .WithMany(r => r.Invoices)
            .HasForeignKey(i => i.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Dorm> Dorms { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
}
