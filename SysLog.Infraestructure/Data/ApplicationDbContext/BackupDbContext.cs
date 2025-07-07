using Microsoft.EntityFrameworkCore;
using SysLog.Repository.Model;

namespace SysLog.Repository.Data.BackupDbContext;

/// <summary>
/// Contexto utilizado para almacenar archivos de backup y para cargar
/// temporalmente los logs restaurados desde dichos ficheros.
/// </summary>
public class BackupDbContext : DbContext
{
    public BackupDbContext(DbContextOptions<BackupDbContext> options) : base(options)
    {
    }

    public DbSet<BackupFile> BackupFile { get; set; }

    // Tablas de logs que se crean al ejecutar los scripts de backup
    public DbSet<Log> Logs { get; set; }
    public DbSet<LogType> LogTypes { get; set; }
    public DbSet<Action> Acctions { get; set; }
    public DbSet<Interface> Interfaces { get; set; }
    public DbSet<Protocol> Protocols { get; set; }
    public DbSet<Signature> Signatures { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Log>().ToTable("logs");
        modelBuilder.Entity<LogType>().ToTable("logs_type");
        modelBuilder.Entity<Action>().ToTable("actions");
        modelBuilder.Entity<Interface>().ToTable("interfaces");
        modelBuilder.Entity<Protocol>().ToTable("protocols");
        modelBuilder.Entity<Signature>().ToTable("signatures");

        modelBuilder.Entity<Log>().HasOne(l => l.LogType)
            .WithOne(l => l.Log)
            .HasForeignKey<LogType>(l => l.LogId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Log>().HasOne(l => l.Action)
            .WithOne(a => a.Log)
            .HasForeignKey<Action>(a => a.LogId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Log>().HasOne(l => l.Interface)
            .WithOne(a => a.Log)
            .HasForeignKey<Interface>(i => i.LogId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Log>().HasOne(l => l.Protocol)
            .WithOne(a => a.Log)
            .HasForeignKey<Protocol>(p => p.LogId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LogType>().HasOne(l => l.Signature)
            .WithOne(s => s.LogType)
            .HasForeignKey<Signature>(s => s.LogTypeId);
    }
}