using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TenHourGuitarist.Data.Entities;

namespace TenHourGuitarist.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonHistory> LessonHistories => Set<LessonHistory>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    public DbSet<Podcast> Podcasts => Set<Podcast>();
    public DbSet<FreeResource> FreeResources => Set<FreeResource>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<SiteVisit> SiteVisits => Set<SiteVisit>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Course
        builder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Instructor)
                .WithMany(u => u.Courses)
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Lesson
        builder.Entity<Lesson>(entity =>
        {
            entity.HasIndex(e => new { e.CourseId, e.Slug }).IsUnique();
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LessonHistory
        builder.Entity<LessonHistory>(entity =>
        {
            entity.HasOne(e => e.User)
                .WithMany(u => u.LessonHistories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Lesson)
                .WithMany(l => l.LessonHistories)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BlogPost
        builder.Entity<BlogPost>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Author)
                .WithMany(u => u.BlogPosts)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.BlogPosts)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // BlogCategory
        builder.Entity<BlogCategory>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Podcast
        builder.Entity<Podcast>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Author)
                .WithMany(u => u.Podcasts)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // FreeResource
        builder.Entity<FreeResource>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Author)
                .WithMany(u => u.FreeResources)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Package
        builder.Entity<Package>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // Subscription
        builder.Entity<Subscription>(entity =>
        {
            entity.HasOne(e => e.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Package)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(e => e.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Order
        builder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderRefId).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Package)
                .WithMany(p => p.Orders)
                .HasForeignKey(e => e.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SiteVisit
        builder.Entity<SiteVisit>(entity =>
        {
            entity.HasIndex(e => e.Date).IsUnique();
        });
    }
}
