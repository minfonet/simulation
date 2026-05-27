using Microsoft.EntityFrameworkCore;
using SimApi.Models;

namespace SimApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SimulationSession> SimulationSessions => Set<SimulationSession>();
    public DbSet<TelemetryRecord> TelemetryRecords => Set<TelemetryRecord>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasOne(u => u.Organization)
                  .WithMany(o => o.Users)
                  .HasForeignKey(u => u.OrganizationId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SimulationSession>(entity =>
        {
            entity.HasOne(s => s.Organization)
                  .WithMany(o => o.Sessions)
                  .HasForeignKey(s => s.OrganizationId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Instructor)
                  .WithMany(u => u.InstructorSessions)
                  .HasForeignKey(s => s.InstructorId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(s => s.Trainee)
                  .WithMany(u => u.TraineeSessions)
                  .HasForeignKey(s => s.TraineeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TelemetryRecord>(entity =>
        {
            entity.HasOne(t => t.Session)
                  .WithMany(s => s.TelemetryRecords)
                  .HasForeignKey(t => t.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(t => t.SessionId);
        });

        modelBuilder.Entity<Evaluation>(entity =>
        {
            entity.HasOne(e => e.Session)
                  .WithOne(s => s.Evaluation)
                  .HasForeignKey<Evaluation>(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Instructor)
                  .WithMany()
                  .HasForeignKey(e => e.InstructorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
