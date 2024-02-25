using LytharBackend.Models;
using Microsoft.EntityFrameworkCore;

public class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Unique constraints
        builder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        builder.Entity<Channel>()
            .HasIndex(c => c.Name)
            .IsUnique();

        // Many-to-many relationship for Messages
        builder.Entity<Message>()
            .HasOne(e => e.Channel)
            .WithMany()
            .HasForeignKey(e => e.ChannelId)
            .IsRequired();

        builder.Entity<Message>()
            .HasOne(e => e.Author)
            .WithMany()
            .HasForeignKey(e => e.AuthorId)
            .IsRequired();
    }
}