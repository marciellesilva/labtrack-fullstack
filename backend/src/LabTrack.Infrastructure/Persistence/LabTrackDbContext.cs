using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Infrastructure.Persistence;

public class LabTrackDbContext(DbContextOptions<LabTrackDbContext> options) : DbContext(options)
{
    public DbSet<Sample> Samples => Set<Sample>();
    public DbSet<AnalysisResult> AnalysisResults => Set<AnalysisResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sample>(e =>
        {
            e.ToTable("Samples");
            e.HasKey(s => s.Id);
            e.Property(s => s.Code).HasMaxLength(20).IsRequired();
            e.HasIndex(s => s.Code).IsUnique();
            e.Property(s => s.ProductName).HasMaxLength(100).IsRequired();
            e.Property(s => s.BatchNumber).HasMaxLength(30).IsRequired();
            e.HasIndex(s => s.BatchNumber);
            e.Property(s => s.Notes).HasMaxLength(500);
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
            e.HasMany(s => s.Results)
                .WithOne(r => r.Sample)
                .HasForeignKey(r => r.SampleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnalysisResult>(e =>
        {
            e.ToTable("AnalysisResults");
            e.HasKey(r => r.Id);
            e.Property(r => r.Parameter).HasMaxLength(50).IsRequired();
            e.Property(r => r.Unit).HasMaxLength(20);
            e.Property(r => r.Analyst).HasMaxLength(100).IsRequired();
            e.Property(r => r.Value).HasPrecision(18, 4);
            e.Property(r => r.MinLimit).HasPrecision(18, 4);
            e.Property(r => r.MaxLimit).HasPrecision(18, 4);
            e.Ignore(r => r.IsWithinSpec); // calculado em memória
            e.HasIndex(r => r.AnalyzedAt);
        });

        SeedData.Apply(modelBuilder);
    }
}
