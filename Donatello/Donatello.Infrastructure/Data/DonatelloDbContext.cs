using System;
using Microsoft.EntityFrameworkCore;
using Donatello.Core.Models;
using Donatello.Core.Enums;

namespace Donatello.Infrastructure;

public class DonatelloDbContext : DbContext
{
    public DonatelloDbContext(DbContextOptions<DonatelloDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Payment> Payments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TCNumber).IsRequired().HasMaxLength(11);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.TCNumber).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Student Configuration
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StudentNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.StudentNumber).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Students)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Course Configuration
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.LicenseCategory).HasConversion<int>();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Enrollment Configuration
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasOne(e => e.Student)
                  .WithMany(s => s.Enrollments)
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentType).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.HasOne(e => e.Enrollment)
                  .WithMany(en => en.Payments)
                  .HasForeignKey(e => e.EnrollmentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Seed Data
        SeedData(modelBuilder);
    }

    
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Courses
        var courseA = new Course
        {
            Id = new Guid("11111111-1111-1111-1111-111111111111"),
            Name = "Motorsiklet Ehliyeti (A Class)",
            LicenseCategory = LicenseCategory.A,
            Price = 2500m,
            TheoryHours = 12,
            PracticeHours = 8,
            Duration = 30,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var courseB = new Course
        {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            Name = "Otomobil Ehliyeti (B Class)",
            LicenseCategory = LicenseCategory.B,
            Price = 3500m,
            TheoryHours = 24,
            PracticeHours = 16,
            Duration = 45,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        modelBuilder.Entity<Course>().HasData(courseA, courseB);
    }
}