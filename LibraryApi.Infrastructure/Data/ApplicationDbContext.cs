using LibraryApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;
    public DbSet<Loan> Loans { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.LastName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.BirthDate).IsRequired();

            entity.HasMany(a => a.Books)
                  .WithOne(b => b.Author)
                  .HasForeignKey(b => b.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict); 
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Isbn).IsRequired().HasMaxLength(20);
            entity.HasIndex(b => b.Isbn).IsUnique(); 
            entity.Property(b => b.PublishedYear).IsRequired();
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(m => m.LastName).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(m => m.Email).IsUnique(); 
            entity.Property(m => m.MembershipDate).IsRequired();
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.LoanDate).IsRequired();
            entity.Property(l => l.ReturnDate).IsRequired(false); 

            entity.HasOne(l => l.Book)
                  .WithMany(b => b.Loans)
                  .HasForeignKey(l => l.BookId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Member)
                  .WithMany(m => m.Loans)
                  .HasForeignKey(l => l.MemberId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}