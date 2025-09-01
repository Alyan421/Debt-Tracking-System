using Debt_Tracking_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Debt_Tracking_System.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.TotalDebt).HasColumnType("numeric(18,2)");
            entity.Property(e => e.CreatedAt).HasColumnType("date");
        });

        // Configure Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("numeric(18,2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Date).HasColumnType("date");
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}