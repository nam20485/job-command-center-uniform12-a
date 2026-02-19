using Microsoft.EntityFrameworkCore;
using JobCommandCenter.Shared.Models;

namespace JobCommandCenter.Data;

/// <summary>
/// Entity Framework Core DbContext for the Job Command Center application.
/// </summary>
public class JobCommandCenterDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the DbContext.
    /// </summary>
    public JobCommandCenterDbContext(DbContextOptions<JobCommandCenterDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Jobs table.
    /// </summary>
    public DbSet<Job> Jobs => Set<Job>();

    /// <summary>
    /// History logs table.
    /// </summary>
    public DbSet<HistoryLog> HistoryLogs => Set<HistoryLog>();

    /// <summary>
    /// Scoring configurations table.
    /// </summary>
    public DbSet<ScoringConfig> ScoringConfigs => Set<ScoringConfig>();

    /// <summary>
    /// Application settings table.
    /// </summary>
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();

    /// <summary>
    /// Configures the entity models.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Job entity configuration
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LinkedInJobId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DateFound);
            entity.HasIndex(e => e.Score);

            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Company).IsRequired().HasMaxLength(500);
            entity.Property(e => e.LinkedInJobId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.JobUrl).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Salary).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(5000);

            entity.HasMany(e => e.HistoryLogs)
                .WithOne(h => h.Job)
                .HasForeignKey(h => h.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // HistoryLog entity configuration
        modelBuilder.Entity<HistoryLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.Timestamp);

            entity.Property(e => e.ChangedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(1000);
        });

        // ScoringConfig entity configuration
        modelBuilder.Entity<ScoringConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IsActive);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Keyword).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });

        // AppSettings entity configuration
        modelBuilder.Entity<AppSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.Category);

            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
        });
    }
}
