using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Entities;

public sealed class CineVaultDbContext : DbContext
{
    public required DbSet<Movie> Movies { get; set; }
    public required DbSet<Review> Reviews { get; set; }
    public required DbSet<User> Users { get; set; }
    public required DbSet<Like> Likes { get; set; }
    public required DbSet<Actor> Actors { get; set; }

    public CineVaultDbContext(DbContextOptions<CineVaultDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // зверху до завдання зі зв'язками
        
        modelBuilder.Entity<Movie>()
            .Property(m => m.Title)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<Movie>()
            .Property(m => m.Genre)
            .HasMaxLength(50);

        modelBuilder.Entity<Movie>()
            .Property(m => m.Director)
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // зверху до четвертого завдання

        modelBuilder.Entity<Movie>()
            .HasIndex(m => m.Title)
            .IsUnique();

        modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.ReviewId })
            .IsUnique();

        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.UserId, r.MovieId })
            .IsUnique();

        // зверху до сьомого завдання

        modelBuilder.Entity<Movie>()
            .Property(m => m.Description)
            .HasMaxLength(1000);

        modelBuilder.Entity<Review>()
            .Property(r => r.Comment)
            .HasMaxLength(1000);

        modelBuilder.Entity<User>()
            .Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.Password)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Actor>()
            .Property(a => a.FullName)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Actor>()
            .Property(a => a.Biography)
            .HasMaxLength(2000);

        // зверху до восьмого завдання

        modelBuilder.Entity<Actor>().HasQueryFilter(a => !a.IsDeleted);
        modelBuilder.Entity<Like>().HasQueryFilter(l => !l.IsDeleted);
        modelBuilder.Entity<Movie>().HasQueryFilter(m => !m.IsDeleted);
        modelBuilder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);

        // зверху до одинадцятого завдання
    }
}